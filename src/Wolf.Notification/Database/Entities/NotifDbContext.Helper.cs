using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wolf.Notification.Database.Entities
{
    public partial class NotifDbContext : DbContext
    {
        public async Task<Recipient> FindRecipientAsync(Recipient recipientIn)
        {
            return await this.Recipients.FirstOrDefaultAsync(r => r.Address == recipientIn.Address && r.Name == recipientIn.Name);
        }

        public async Task<Recipient> FindRecipientOrUseThis(Recipient recipientIn)
        {
            Recipient recipientOut = await FindRecipientAsync(recipientIn);
            return recipientOut ?? recipientIn;
        }

        public async Task<Recipient> FindOrAddRecipientAsync(Recipient recipientIn)
        {
            Recipient existingRecipient = await this.FindRecipientAsync(recipientIn);
            if (null == existingRecipient)
            {
                recipientIn.RecipientId = 0;
                this.Recipients.Add(recipientIn);
                return recipientIn;
            }
            return existingRecipient;
        }

        public async Task PossiblyDeleteRecipientForTemplateSender(Recipient recipeint, Guid templateId)
        {
            if (!await this.MessageRecipients.AnyAsync(p => p.RecipientId == recipeint.RecipientId) &&
                !await this.Messages.AnyAsync(p => p.FromRecipientId == recipeint.RecipientId) &&
                !await this.TemplateRecipients.AnyAsync(p => p.RecipientId == recipeint.RecipientId) &&
                !await this.Templates.AnyAsync(p => p.DefaultFromRecipientId == recipeint.RecipientId && p.TemplateId != templateId))
            {
                this.Recipients.Remove(recipeint);
            }
        }

        public async Task PossiblyDeleteRecipientForTemplate(Recipient recipeint, Guid templateId)
        {
            if (!await this.MessageRecipients.AnyAsync(p => p.RecipientId == recipeint.RecipientId) &&
                !await this.Messages.AnyAsync(p => p.FromRecipientId == recipeint.RecipientId) &&
                !await this.TemplateRecipients.AnyAsync(p => p.RecipientId == recipeint.RecipientId && p.TemplateId != templateId) &&
                !await this.Templates.AnyAsync(p => p.DefaultFromRecipientId == recipeint.RecipientId))
            {
                this.Recipients.Remove(recipeint);
            }
        }
    }
}
