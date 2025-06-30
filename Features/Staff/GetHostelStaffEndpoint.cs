using FastEndpoints;
using HostelManagementSystemApi.Features.Common;
using HostelManagementSystemApi.Features.Staff.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace HostelManagementSystemApi.Features.Staff
{
    public class GetHostelStaffEndpoint : Endpoint<GetHostelStaffRequest, PagedResponse<StaffResponse>>
    {
        private readonly ApplicationDbContext _context;

        public GetHostelStaffEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/hostels/{HostelID}/staff");
            Roles("Vendor");
        }

        public override async Task HandleAsync(GetHostelStaffRequest req, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                await SendUnauthorizedAsync(ct);
                return;
            }

            var vendor = await _context.Vendors.AsNoTracking().FirstOrDefaultAsync(v => v.UserID == int.Parse(userId), ct);
            if (vendor == null)
            {
                await SendForbiddenAsync(ct);
                return;
            }

            var hostelId = Route<int>("HostelID");
            var hostel = await _context.Hostels.AsNoTracking().FirstOrDefaultAsync(h => h.HostelID == hostelId, ct);

            if (hostel == null || hostel.VendorID != vendor.VendorID)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            var query = _context.Staff
                .Where(s => s.HostelID == hostelId)
                .Include(s => s.User)!.ThenInclude(u => u!.Role)
                .Include(s => s.Hostel)
                .AsNoTracking();

            // Apply sorting
            if (!string.IsNullOrEmpty(req.SortBy))
            {
                var isDescending = string.Equals(req.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
                switch (req.SortBy.ToLower())
                {
                    case "name":
                        query = isDescending ? query.OrderByDescending(s => s.User != null ? s.User.Name : string.Empty) : query.OrderBy(s => s.User != null ? s.User.Name : string.Empty);
                        break;
                    default:
                        query = query.OrderBy(s => s.User != null ? s.User.Name : string.Empty);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(s => s.User != null ? s.User.Name : string.Empty);
            }

            var totalCount = await query.CountAsync(ct);

            var staff = await query
                .Select(s => new StaffResponse
                {
                    StaffID = s.StaffID,
                    UserID = s.UserID,
                    Name = s.User != null ? s.User.Name : string.Empty,
                    Email = s.User != null ? s.User.Email : string.Empty,
                    Role = s.User != null && s.User.Role != null ? s.User.Role.RoleName : string.Empty,
                    Salary = s.Salary,
                    JoinDate = s.JoinDate,
                    HostelID = s.HostelID,
                    HostelName = s.Hostel != null ? s.Hostel.Name : string.Empty
                })
                .Skip((req.PageNumber - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync(ct);

            var response = new PagedResponse<StaffResponse>(staff, req.PageNumber, req.PageSize, totalCount);

            await SendAsync(response, 200, ct);
        }
    }
}
