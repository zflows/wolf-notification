SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [notif].[template](
	[template_id] [uniqueidentifier] NOT NULL,
	[template_name] [varchar](512) NOT NULL,
	[template_subject] [nvarchar](max) NULL,
	[template_body] [nvarchar](max) NULL,
	[default_provider_code] [varchar](256) NULL,
	[default_from_recipient_id] [bigint] NULL,
 CONSTRAINT [PK_template] PRIMARY KEY CLUSTERED 
(
	[template_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [notif].[template]  WITH CHECK ADD  CONSTRAINT [FK_template_provider] FOREIGN KEY([default_provider_code])
REFERENCES [notif].[provider] ([provider_code])
GO

ALTER TABLE [notif].[template] CHECK CONSTRAINT [FK_template_provider]
GO

ALTER TABLE [notif].[template]  WITH CHECK ADD  CONSTRAINT [FK_template_recipient] FOREIGN KEY([default_from_recipient_id])
REFERENCES [notif].[recipient] ([recipient_id])
GO

ALTER TABLE [notif].[template] CHECK CONSTRAINT [FK_template_recipient]
GO


CREATE UNIQUE NONCLUSTERED INDEX IU_template ON notif.template (template_name)