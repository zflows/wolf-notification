SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[notif].[recipient]'))
Begin
	CREATE TABLE [notif].[recipient](
		[recipient_id] [bigint] IDENTITY(1,1) NOT NULL,
		[address] [nvarchar](1024) NOT NULL,
		[name] [nvarchar](1024) NULL,
	 CONSTRAINT [PK_recipient] PRIMARY KEY CLUSTERED 
	(
		[recipient_id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
End
