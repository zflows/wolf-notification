/****** Object:  StoredProcedure [notif].[stats_time_series]    Script Date: 4/12/2021 10:57:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER PROCEDURE [notif].[stats_time_series]
	@days int= 30,
	@topcount int=5
AS
BEGIN
	create table #tmp_templ (template_id uniqueidentifier not null, message_count int not null)
	create table #tmp_dates (day_date Date not null)

	insert into #tmp_templ
		select top(@topcount) m.template_id, COUNT(m.message_id) message_count
		from notif.message m 
		where m.date_created>DATEADD(DAY, -@days, CAST(GETDATE() as date))
		group by m.template_id
		order by message_count desc

	insert into #tmp_dates
		Select DATEADD(DAY, -s.number, CAST(GETDATE() as date)) day_date FROM master..spt_values s
		WHERE type = 'P' AND number BETWEEN 0 AND @days-1
		order by 1

	select t.template_id, d.day_date, count(m.message_id) message_count
	FROM  #tmp_templ t
	cross join #tmp_dates d
	LEFT OUTER JOIN notif.message m
		ON CAST(m.date_created as date)=d.day_date
		AND m.template_id=t.template_id
	GROUP BY  t.template_id, d.day_date
	ORDER BY  t.template_id, d.day_date

	drop table #tmp_templ
	drop table #tmp_dates
END
