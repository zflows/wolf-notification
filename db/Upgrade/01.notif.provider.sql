IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[notif].[provider]'))
Begin

	CREATE TABLE [notif].[provider](
		[provider_code] [varchar](256) NOT NULL,
		[description] [nvarchar](max) NULL,
	 CONSTRAINT [PK_provider] PRIMARY KEY CLUSTERED 
	(
		[provider_code] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

End
go

IF NOT EXISTS (SELECT * FROM [notif].[provider] WHERE [provider_code] = 'email')
 insert into [notif].[provider] (provider_code, description) values ('email', 'SMTP Email Provider')