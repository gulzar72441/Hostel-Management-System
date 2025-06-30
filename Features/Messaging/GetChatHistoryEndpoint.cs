using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using HostelManagementSystemApi.Persistence;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Authorization;
using FluentValidation;

namespace HostelManagementSystemApi.Features.Messaging
{
    [Authorize]
    public class GetChatHistoryRequest
    {
        public int OtherUserId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class GetChatHistoryValidator : Validator<GetChatHistoryRequest>
    {
        public GetChatHistoryValidator()
        {
            RuleFor(x => x.OtherUserId).GreaterThan(0);
            RuleFor(x => x.Page).GreaterThan(0);
            RuleFor(x => x.PageSize).GreaterThan(0);
        }
    }

    public class ChatMessageDto
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class GetChatHistoryResponse
    {
        public List<ChatMessageDto> Messages { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }

    public class GetChatHistoryEndpoint : Endpoint<GetChatHistoryRequest, GetChatHistoryResponse>
    {
        private readonly ApplicationDbContext _context;

        public GetChatHistoryEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/messaging/history/{OtherUserId}");
            Roles("Student", "Vendor");
        }

        public override async Task HandleAsync(GetChatHistoryRequest req, CancellationToken ct)
        {
            var currentUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(currentUserIdString, out var currentUserId))
            {
                await SendUnauthorizedAsync(ct);
                return;
            }

            var query = _context.ChatMessages
                .Where(m => (m.SenderId == currentUserId && m.RecipientId == req.OtherUserId) ||
                             (m.SenderId == req.OtherUserId && m.RecipientId == currentUserId))
                .OrderBy(m => m.Timestamp);

            var totalCount = await query.CountAsync(ct);

            var messages = await query
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
                .Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    RecipientId = m.RecipientId,
                    Content = m.Content,
                    Timestamp = m.Timestamp
                })
                .ToListAsync(ct);

            await SendAsync(new GetChatHistoryResponse
            {
                Messages = messages,
                Page = req.Page,
                PageSize = req.PageSize,
                TotalCount = totalCount
            }, cancellation: ct);
        }
    }
}
