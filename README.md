# Wolf Notification Service API

Template-based notification engine
Currently supports e-mail notifications.

List of third-party libraries:

  - **[DotLiquid](http://dotliquidmarkup.org)** is a templating system. [How to use liquid markeup. ](https://shopify.github.io/liquid/)
  
  - **[Enity Framework Core](https://docs.microsoft.com/en-us/ef/core/)**
  
  - **SQL Server** used as a data storage
  
  - **[Redis](https://redis.io)** used as a message broker
  
  - **[Serilog](https://serilog.net)** is a library for logging.
  

### Wolf.Notification

Wolf.Notification is an ASP.NET WebAPI application that provides an API for creating and sending messages.

The API has the following settings:

  - `NotificationDbContext` is a connection string to SQL Server
  
  - `Redis` is a connection string to a Redis message broker.
  
  - `Logging` is a default ASP.NET MVC section
  

Example:
```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "NotificationDbContext": "Server=localhost;Initial Catalog=notification_v1;Persist Security Info=False;User ID=sa;Password=MP3cxJ6ptB3R93VP;Connection Timeout=30;",
    "Redis": "localhost"
  }
}
```


### Wolf.Notification.EmailSender

Wolf.Notification.EmailSender is a console application that listens for an `Runner:QueueChannelName` queue and sends e-mail messages.

The application has the following settings:
  
  - `Logging` is a default .Net Core section
  
  - `Serilog` is a configuration for the Serilog librar. [See Formatting Output](https://github.com/serilog/serilog/wiki/Formatting-Output) 
  
  
  - `NotifApi` properties to connect to Wolf.Notification API  
  
  - `Runner:QueueConnectionString` is a connection string to a Redis message broker.
  
  - `Runner:QueueChannelName` is a channel that the application will listen to and wait for the message.
  
  - `Smtp` is a SMTP server configuration.

```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "Serilog": {
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console",
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <s:{SourceContext}>{NewLine}{Exception}"
        }
      }
    ]
  },
  "NotifApi": {
    "BaseUrl": "http://localhost:5380",
    "UserAgentName": "EmailSender",
    "AuthOptions": {
      "StsUrl": "https://devid.myaccount.com",
      "ClientId": "notif_sender",
      "ClientSecret": "HIDDEN",
      "Scope": "notif_api"
    }
  },
  "Runner": {
    "QueueConnectionString": "localhost",
    "QueueChannelName": "email"
  },
  "Smtp": {
    "Server": "smtp.gmail.com",
    "Port": 587,
    "Username": "2386612@gmail.com",
    "Password": "1234",
    "FromEmail": "2386612@gmail.com"
  }
}
```


    
### Setup

1. If not using docker, install Redis Server. There are many ports for windows. Here is one of them: https://github.com/microsoftarchive/redis/releases/tag/win-3.2.100

2. You need to execute `src/Wolf.Notification/Sql` scripts  before build and deployin the applications. The `src/Wolf.Notification/Sql` contains the following scripts:

- `src/Wolf.Notification/Sql/000_init_db.sql` - create a new database (*if you already have a database, you can skip this script*)

- `src/Wolf.Notification/Sql/001_[audit]_audit_schema.sql` - create an audit schema  (*if you do not want to have audit tables, you can skip this script*)

- `src/Wolf.Notification/Sql/002_template.sql` **(MANDATORY)**

- `src/Wolf.Notification/Sql/003_[audit]_template.sql` is a script to create an audit table for the `template` table. *(Optional)*

- `src/Wolf.Notification/Sql/004_template_default_recipient.sql` **(MANDATORY)**

- `src/Wolf.Notification/Sql/005_[audit]_template_default_recipient.sql` is a script to create an audit table for the `template_default_recipient` table. *(Optional)*

- `src/Wolf.Notification/Sql/006_message.sql`  **(MANDATORY)**

- `src/Wolf.Notification/Sql/007_[audit]_message.sql` is a script to create an audit table for the `message` table. *(Optional)*

- `src/Wolf.Notification/Sql/008_recipient.sql`  **(MANDATORY)**

- `src/Wolf.Notification/Sql/009_[audit]_recipient.sql` is a script to create an audit table for the `recipient` table. *(Optional)*

### How to use

1. You need to create an template using `POST /template`. [See swagger](http://localhost:8080/swagger/index.html) 

See example,
```
{
  "name": "list of services",
  "subject": "Notification for {{username}}",
  "body": "<b>Hello {{username}},</b> We have the following applications:<ul>{% for app in apps %}<li>{{app.name}} with ID = {{app.ID}}</li>{% endfor %}</ul>"
}
```

2. You need to run the Wolf.Notification.EmailSender application

3. You can send message using `POST /message/send/template/{templateId}`. Provider parameter must be `email`

See example,
```
{
  "tokenValues": {
    "username": "Greg",
    "apps": [
        {
            "name": "Wolf Forms",
            "ID": 111
        },
        {
            "name": "Wolf Flows",
            "ID": 222
        },
        {
            "name": "Wolf Notifications",
            "ID": 333
        },
    ]
},
  "recipients": [
    "gzinger@zbitinc.com"
  ]
}
```

### How to use Docker Compose 

!!! **Before using, you need to do the following steps:**

- Setup Database Scripts

- Change `ConnectionStrings__NotificationDbContext` to your

- Change `Smtp__Username`, `Smtp__Password`, `Smtp__FromEmail` to your settings

```
docker-compose build
```

```
docker-compose up
```

