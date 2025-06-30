using FastEndpoints;
using HostelManagementSystemApi.Features.Common;
using HostelManagementSystemApi.Features.Payments.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace HostelManagementSystemApi.Features.Payments
{
    public class GetMyPaymentsEndpoint : Endpoint<GetMyPaymentsRequest, PagedResponse<PaymentResponse>>
    {
        private readonly ApplicationDbContext _context;

        public GetMyPaymentsEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/payments/my-payments");
            Roles("Student");
        }

        public override async Task HandleAsync(GetMyPaymentsRequest req, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                await SendUnauthorizedAsync(ct);
                return;
            }

            var student = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.UserID == int.Parse(userId), ct);
            if (student == null)
            {
                await SendForbiddenAsync(ct);
                return;
            }

            var query = _context.Payments
                .Where(p => p.StudentID == student.StudentID)
                .AsNoTracking();

            // Apply filtering
            if (!string.IsNullOrEmpty(req.Status))
            {
                query = query.Where(p => p.Status == req.Status);
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(req.SortBy))
            {
                var isDescending = string.Equals(req.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
                switch (req.SortBy.ToLower())
                {
                    case "date":
                        query = isDescending ? query.OrderByDescending(p => p.DueDate) : query.OrderBy(p => p.DueDate);
                        break;
                    case "amount":
                        query = isDescending ? query.OrderByDescending(p => p.Amount) : query.OrderBy(p => p.Amount);
                        break;
                    default:
                        query = query.OrderByDescending(p => p.DueDate);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(p => p.DueDate);
            }

            var totalCount = await query.CountAsync(ct);

            var payments = await query
                .Include(p => p.Student)!.ThenInclude(s => s!.User)
                .Include(p => p.Hostel)
                .Select(p => new PaymentResponse
                {
                    PaymentID = p.PaymentID,
                    StudentID = p.StudentID,
                    StudentName = p.Student!.User!.Name,
                    HostelID = p.HostelID,
                    HostelName = p.Hostel!.Name,
                    Amount = p.Amount,
                    DueDate = p.DueDate,
                    PaidDate = p.PaidDate,
                    Status = p.Status,
                    Method = p.Method,
                    ReceiptURL = p.ReceiptURL
                })
                .Skip((req.PageNumber - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync(ct);

            var response = new PagedResponse<PaymentResponse>(payments, req.PageNumber, req.PageSize, totalCount);

            await SendAsync(response, 200, ct);
        }
    }
}
