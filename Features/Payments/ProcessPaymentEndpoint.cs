using FastEndpoints;
using HostelManagementSystemApi.Features.Payments.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace HostelManagementSystemApi.Features.Payments
{
    public class ProcessPaymentEndpoint : Endpoint<ProcessPaymentRequest, PaymentResponse>
    {
        private readonly ApplicationDbContext _context;

        public ProcessPaymentEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Put("/api/payments/{PaymentID}/pay");
            Roles("Student");
        }

        public override async Task HandleAsync(ProcessPaymentRequest req, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                await SendUnauthorizedAsync(ct);
                return;
            }

            var student = await _context.Students
                .Include(s => s.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserID == int.Parse(userId), ct);

            if (student == null || student.User == null)
            {
                await SendForbiddenAsync(ct);
                return;
            }

            var paymentId = Route<int>("PaymentID");
            var payment = await _context.Payments
                .Include(p => p.Hostel)
                .FirstOrDefaultAsync(p => p.PaymentID == paymentId && p.StudentID == student.StudentID, ct);

            if (payment == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            if (payment.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase))
            {
                AddError("This payment has already been processed.");
                await SendErrorsAsync(400, ct);
                return;
            }

            payment.Status = "Paid";
            payment.PaidDate = DateTime.UtcNow;
            payment.Method = req.Method;
            payment.ReceiptURL = $"/receipts/{payment.PaymentID}/{Guid.NewGuid()}"; // Placeholder URL

            await _context.SaveChangesAsync(ct);

            var response = new PaymentResponse
            {
                PaymentID = payment.PaymentID,
                StudentID = payment.StudentID,
                StudentName = student.User.Name,
                HostelID = payment.HostelID,
                HostelName = payment.Hostel!.Name,
                Amount = payment.Amount,
                DueDate = payment.DueDate,
                PaidDate = payment.PaidDate,
                Status = payment.Status,
                Method = payment.Method,
                ReceiptURL = payment.ReceiptURL
            };

            await SendAsync(response, 200, ct);
        }
    }
}
