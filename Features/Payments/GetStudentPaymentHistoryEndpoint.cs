using FastEndpoints;
using HostelManagementSystemApi.Features.Payments.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Payments
{
    public class GetStudentPaymentHistoryEndpoint : EndpointWithoutRequest<List<PaymentResponse>>
    {
        private readonly ApplicationDbContext _context;

        public GetStudentPaymentHistoryEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/payments/my-history");
            Roles("Student");
        }

        public override async Task HandleAsync(CancellationToken ct)
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

            var payments = await _context.Payments
                .Where(p => p.StudentID == student.StudentID)
                .AsNoTracking()
                .Select(p => new PaymentResponse
                {
                    PaymentID = p.PaymentID,
                    Amount = p.Amount,
                    DueDate = p.DueDate,
                    PaidDate = p.PaidDate,
                    Status = p.Status,
                    Method = p.Method,
                    ReceiptURL = p.ReceiptURL
                })
                .ToListAsync(ct);

            await SendAsync(payments, 200, ct);
        }
    }
}
