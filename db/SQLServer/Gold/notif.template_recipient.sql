SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [notif].[template_recipient](
	[tr_id] [bigint] IDENTITY(1,1) NOT NULL,
	[template_id] [uniqueidentifier] NOT NULL,
	[recipient_id] [bigint] NOT NULL,
	[type_code] [varchar](25) NOT NULL,
 CONSTRAINT [PK_template_recipient] PRIMARY KEY CLUSTERED 
(
	[tr_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [notif].[template_recipient]  WITH CHECK ADD  CONSTRAINT [FK_template_recipient_recipient] FOREIGN KEY([recipient_id])
REFERENCES [notif].[recipient] ([recipient_id])
GO

ALTER TABLE [notif].[template_recipient] CHECK CONSTRAINT [FK_template_recipient_recipient]
GO

ALTER TABLE [notif].[template_recipient]  WITH CHECK ADD  CONSTRAINT [FK_template_recipient_recipient_type] FOREIGN KEY([type_code])
REFERENCES [notif].[recipient_type] ([type_code])
GO

ALTER TABLE [notif].[template_recipient] CHECK CONSTRAINT [FK_template_recipient_recipient_type]
GO

ALTER TABLE [notif].[template_recipient]  WITH CHECK ADD  CONSTRAINT [FK_template_recipient_template] FOREIGN KEY([template_id])
REFERENCES [notif].[template] ([template_id])
GO

ALTER TABLE [notif].[template_recipient] CHECK CONSTRAINT [FK_template_recipient_template]
GO

