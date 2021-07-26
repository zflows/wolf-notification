SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[notif].[generated_message]'))
Begin
	CREATE TABLE [notif].[generated_message](
		[message_id] [uniqueidentifier] NOT NULL,
		[subject] [nvarchar](max) NULL,
		[body] [nvarchar](max) NULL,
	 CONSTRAINT [PK_generated_message] PRIMARY KEY CLUSTERED 
	(
		[message_id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

	ALTER TABLE [notif].[generated_message]  WITH CHECK ADD  CONSTRAINT [FK_generated_message_message] FOREIGN KEY([message_id])
	REFERENCES [notif].[message] ([message_id])

	ALTER TABLE [notif].[generated_message] CHECK CONSTRAINT [FK_generated_message_message]
End