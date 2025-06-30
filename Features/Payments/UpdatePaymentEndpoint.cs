using FastEndpoints;
using HostelManagementSystemApi.Features.Payments.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Payments
{
    public class UpdatePaymentEndpoint : Endpoint<UpdatePaymentRequest, VendorPaymentResponse>
    {
        private readonly ApplicationDbContext _context;

        public UpdatePaymentEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Put("/api/payments/{PaymentID}");
            Roles("Vendor");
        }

        public override async Task HandleAsync(UpdatePaymentRequest req, CancellationToken ct)
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

            var paymentId = Route<int>("PaymentID");
            var payment = await _context.Payments
                .Include(p => p.Hostel)
                .Include(p => p.Student)!.ThenInclude(s => s!.User)
                .FirstOrDefaultAsync(p => p.PaymentID == paymentId, ct);

            if (payment == null || payment.Hostel == null || payment.Hostel.VendorID != vendor.VendorID)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            payment.Status = req.Status;
            payment.Method = req.Method;
            payment.PaidDate = req.PaidDate;

            await _context.SaveChangesAsync(ct);

            var response = new VendorPaymentResponse
            {
                PaymentID = payment.PaymentID,
                StudentName = payment.Student!.User!.Name,
                HostelName = payment.Hostel.Name,
                Amount = payment.Amount,
                DueDate = payment.DueDate,
                Status = payment.Status,
                PaymentDate = payment.PaidDate
            };

            await SendAsync(response, 200, ct);
        }
    }
}
