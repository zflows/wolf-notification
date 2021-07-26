SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[notif].[template_recipient]'))
Begin
	CREATE TABLE [notif].[template_recipient](
		[tr_id] [bigint] IDENTITY(1,1) NOT NULL,
		[template_id] [uniqueidentifier] NOT NULL,
		[recipient_id] [bigint] NOT NULL,
		[type_code] [varchar](25) NOT NULL,
	 CONSTRAINT [PK_template_recipient] PRIMARY KEY CLUSTERED 
	(
		[tr_id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [notif].[template_recipient]  WITH CHECK ADD  CONSTRAINT [FK_template_recipient_recipient] FOREIGN KEY([recipient_id])
	REFERENCES [notif].[recipient] ([recipient_id])

	ALTER TABLE [notif].[template_recipient] CHECK CONSTRAINT [FK_template_recipient_recipient]

	ALTER TABLE [notif].[template_recipient]  WITH CHECK ADD  CONSTRAINT [FK_template_recipient_recipient_type] FOREIGN KEY([type_code])
	REFERENCES [notif].[recipient_type] ([type_code])

	ALTER TABLE [notif].[template_recipient] CHECK CONSTRAINT [FK_template_recipient_recipient_type]

	ALTER TABLE [notif].[template_recipient]  WITH CHECK ADD  CONSTRAINT [FK_template_recipient_template] FOREIGN KEY([template_id])
	REFERENCES [notif].[template] ([template_id])

	ALTER TABLE [notif].[template_recipient] CHECK CONSTRAINT [FK_template_recipient_template]
End