using FastEndpoints;
using HostelManagementSystemApi.Domain;
using HostelManagementSystemApi.Features.Payments.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;

namespace HostelManagementSystemApi.Features.Payments
{
    public class GetVendorPaymentsRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class PaginatedResponse<T>
    {
        public List<T> Results { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
    }

    public class GetVendorPaymentsEndpoint : Endpoint<GetVendorPaymentsRequest, PaginatedResponse<VendorPaymentResponse>>
    {
        private readonly ApplicationDbContext _context;

        public GetVendorPaymentsEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/payments/vendor-payments");
            Roles("Vendor");
        }

        public override async Task HandleAsync(GetVendorPaymentsRequest req, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                await SendUnauthorizedAsync(ct);
                return;
            }

            var vendor = await _context.Vendors.AsNoTracking()
                .FirstOrDefaultAsync(v => v.UserID == int.Parse(userId), ct);

            if (vendor == null)
            {
                await SendForbiddenAsync(ct);
                return;
            }

            var hostelIds = await _context.Hostels.AsNoTracking()
                .Where(h => h.VendorID == vendor.VendorID)
                .Select(h => h.HostelID)
                .ToListAsync(ct);

            if (!hostelIds.Any())
            {
                await SendAsync(new PaginatedResponse<VendorPaymentResponse>(), 200, ct);
                return;
            }

            var query = _context.Payments.AsNoTracking()
                .Where(p => hostelIds.Contains(p.HostelID))
                .Select(p => new VendorPaymentResponse
                {
                    PaymentID = p.PaymentID,
                    StudentName = (p.Student != null && p.Student.User != null) ? p.Student.User.Name : string.Empty,
                    HostelName = p.Hostel != null ? p.Hostel.Name : string.Empty,
                    Amount = p.Amount,
                    DueDate = p.DueDate,
                    Status = p.Status,
                    PaymentDate = p.PaymentTransactions
                        .OrderByDescending(t => t.TransactionDate)
                        .Select(t => (DateTime?)t.TransactionDate)
                        .FirstOrDefault()
                });

            var totalCount = await query.CountAsync(ct);
            var payments = await query.Skip((req.Page - 1) * req.PageSize).Take(req.PageSize).ToListAsync(ct);

            var response = new PaginatedResponse<VendorPaymentResponse>
            {
                Results = payments,
                TotalCount = totalCount,
                Page = req.Page
            };

            await SendAsync(response, 200, ct);
        }
    }
}
