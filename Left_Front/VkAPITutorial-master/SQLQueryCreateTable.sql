USE [vk_db]
GO

/****** Object:  Table [dbo].[users]    Script Date: 07.11.2018 16:09:21 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[users](
	[user_id] [nvarchar](50) NULL,
	[message_sent] [smallint] NULL
) ON [PRIMARY]
GO

GO
ALTER TABLE dbo.users ADD
	photo_id nvarchar(50) NULL,
	verified nvarchar(50) NULL,
	sex nvarchar(50) NULL,
	bdate nvarchar(50) NULL,
	city nvarchar(50) NULL,
	country nvarchar(50) NULL,
	home_town nvarchar(50) NULL,
	has_photo nvarchar(50) NULL,

	can_write_private_message nvarchar(50) NULL,
	can_send_friend_request nvarchar(50) NULL
	
GO
ALTER TABLE dbo.users SET (LOCK_ESCALATION = TABLE)
GO



