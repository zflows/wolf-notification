IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[notif].[recipient_type]'))
Begin
	CREATE TABLE [notif].[recipient_type](
		[type_code] [varchar](25) NOT NULL,
		[description] [nvarchar](max) NULL,
	 CONSTRAINT [PK_recipient_type] PRIMARY KEY CLUSTERED 
	(
		[type_code] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
End
GO

IF NOT EXISTS (SELECT * FROM [notif].[recipient_type] WHERE [type_code] = 'to')
 insert into [notif].[recipient_type] ([type_code], [description]) values ('to', 'TO Recipient')

IF NOT EXISTS (SELECT * FROM [notif].[recipient_type] WHERE [type_code] = 'cc')
 insert into [notif].[recipient_type] ([type_code], [description]) values ('cc', 'CC Recipient')

IF NOT EXISTS (SELECT * FROM [notif].[recipient_type] WHERE [type_code] = 'bcc')
 insert into [notif].[recipient_type] ([type_code], [description]) values ('bcc', 'BCC Recipient')