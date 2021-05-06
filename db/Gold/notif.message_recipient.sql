SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [notif].[message_recipient](
	[mr_id] [bigint] IDENTITY(1,1) NOT NULL,
	[message_id] [uniqueidentifier] NOT NULL,
	[recipient_id] [bigint] NOT NULL,
	[type_code] [varchar](25) NULL,
 CONSTRAINT [PK_message_recipient] PRIMARY KEY CLUSTERED 
(
	[mr_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [notif].[message_recipient]  WITH CHECK ADD  CONSTRAINT [FK_message_recipient_message] FOREIGN KEY([message_id])
REFERENCES [notif].[message] ([message_id])
GO

ALTER TABLE [notif].[message_recipient] CHECK CONSTRAINT [FK_message_recipient_message]
GO

ALTER TABLE [notif].[message_recipient]  WITH CHECK ADD  CONSTRAINT [FK_message_recipient_recipient] FOREIGN KEY([recipient_id])
REFERENCES [notif].[recipient] ([recipient_id])
GO

ALTER TABLE [notif].[message_recipient] CHECK CONSTRAINT [FK_message_recipient_recipient]
GO

ALTER TABLE [notif].[message_recipient]  WITH CHECK ADD  CONSTRAINT [FK_message_recipient_recipient_type] FOREIGN KEY([type_code])
REFERENCES [notif].[recipient_type] ([type_code])
GO

ALTER TABLE [notif].[message_recipient] CHECK CONSTRAINT [FK_message_recipient_recipient_type]
GO