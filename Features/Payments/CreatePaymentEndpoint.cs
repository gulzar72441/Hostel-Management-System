using FastEndpoints;
using HostelManagementSystemApi.Features.Payments.DTOs;
using HostelManagementSystemApi.Domain;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Payments
{
    public class CreatePaymentEndpoint : Endpoint<CreatePaymentRequest, PaymentResponse>
    {
        private readonly ApplicationDbContext _context;

        public CreatePaymentEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Post("/api/payments");
            Roles("Vendor");
        }

        public override async Task HandleAsync(CreatePaymentRequest req, CancellationToken ct)
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

            var hostel = await _context.Hostels.AsNoTracking().FirstOrDefaultAsync(h => h.HostelID == req.HostelID, ct);
            if (hostel == null || hostel.VendorID != vendor.VendorID)
            {
                AddError("Hostel not found or you do not have permission to access it.");
                await SendErrorsAsync(404, ct);
                return;
            }

            var student = await _context.Students.Include(s => s.User).AsNoTracking().FirstOrDefaultAsync(s => s.StudentID == req.StudentID, ct);
            if (student == null || student.HostelID != req.HostelID || student.User == null)
            {
                AddError("Student not found or not registered in this hostel.");
                await SendErrorsAsync(404, ct);
                return;
            }

            var payment = new Payment
            {
                StudentID = req.StudentID,
                HostelID = req.HostelID,
                Amount = req.Amount,
                DueDate = req.DueDate,
                Status = req.Status,
                PaidDate = null,
                Method = string.Empty,
                ReceiptURL = string.Empty
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync(ct);

            var response = new PaymentResponse
            {
                PaymentID = payment.PaymentID,
                StudentID = payment.StudentID,
                StudentName = student.User.Name,
                HostelID = payment.HostelID,
                HostelName = hostel.Name,
                Amount = payment.Amount,
                DueDate = payment.DueDate,
                Status = payment.Status,
                PaidDate = payment.PaidDate,
                Method = payment.Method,
                ReceiptURL = payment.ReceiptURL
            };

            await SendAsync(response, 201, ct);
        }
    }
}
