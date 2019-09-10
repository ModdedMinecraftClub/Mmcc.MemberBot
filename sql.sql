-- Check if table exists --
SELECT count(*) FROM information_schema.TABLES WHERE (TABLE_SCHEMA = @name) AND (TABLE_NAME = 'applications');

-- Create table --
create table applications
(
	AppId int not null auto_increment,
	AppStatus int not null,
	AppTime varchar(50) null,
	AuthorName varchar(50) not null,
	AuthorDiscordId long not null,
	MessageContent varchar(800) null,
    MessageUrl varchar(250) not null,
	ImageUrl varchar(250) null,
	Prefix varchar(5) null,
	primary key (AppId)
);

-- Insert --
insert into applications (AppStatus, AppTime, AuthorName, AuthorDiscordId, MessageContent, MessageUrl, ImageUrl) values (@AppStatus, @AppTime, @AuthorName, @AuthorDiscordId, @MessageContent, @MessageUrl, @ImageUrl);

-- Select all pending --
select * from applications where AppStatus = 0 order by AppId;

-- Select last 20 approved --
select * from applications where AppStatus = 1 order by AppId limit 20;

-- Select last 20 rejected --
select * from applications where AppStatus = 2 order by AppId limit 20;

-- Mark as approved --
update applications
set AppStatus = 1
where AppId = @AppId

-- Mark as rejected --
update applications
set AppStatus = 2
where AppId = @AppId