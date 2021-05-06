SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[notif].[message]'))
Begin
	CREATE TABLE [notif].[message](
		[message_id] [uniqueidentifier] NOT NULL,
		[template_id] [uniqueidentifier] NOT NULL,
		[token_data] [nvarchar](max) NULL,
		[date_created] [datetime] NOT NULL,
		[date_processed] [datetime] NULL,
		[date_sent] [datetime] NULL,
		[provider_code] [varchar](256) NOT NULL,
		[from_recipient_id] [bigint] NOT NULL,
	 CONSTRAINT [message_pk] PRIMARY KEY NONCLUSTERED 
	(
		[message_id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

	ALTER TABLE [notif].[message] ADD  CONSTRAINT [DF__message__message__0C85DE4D]  DEFAULT (newid()) FOR [message_id]

	ALTER TABLE [notif].[message] ADD  CONSTRAINT [DF__message__created__0E6E26BF]  DEFAULT (getdate()) FOR [date_created]

	ALTER TABLE [notif].[message]  WITH CHECK ADD  CONSTRAINT [FK_message_provider] FOREIGN KEY([provider_code])
	REFERENCES [notif].[provider] ([provider_code])

	ALTER TABLE [notif].[message] CHECK CONSTRAINT [FK_message_provider]

	ALTER TABLE [notif].[message]  WITH CHECK ADD  CONSTRAINT [FK_message_recipient] FOREIGN KEY([from_recipient_id])
	REFERENCES [notif].[recipient] ([recipient_id])

	ALTER TABLE [notif].[message] CHECK CONSTRAINT [FK_message_recipient]

	ALTER TABLE [notif].[message]  WITH CHECK ADD  CONSTRAINT [message_template_template_id_fk] FOREIGN KEY([template_id])
	REFERENCES [notif].[template] ([template_id])

	ALTER TABLE [notif].[message] CHECK CONSTRAINT [message_template_template_id_fk]
End