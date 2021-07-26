insert into Wolf.notif.template (template_id, template_name, template_subject, template_body, default_provider_code)
select template_id, template_name, template_subject, template_body, 'email' from OLDDB.notif.template t
  where t.template_id not in (select template_id from Wolf.notif.template) 
  and t.template_name not in (select template_name from Wolf.notif.template) 

insert into Wolf.notif.recipient (address)
select distinct recipient_address from OLDDB.notif.[recipient]

insert into Wolf.notif.message(message_id, template_id, date_created, date_processed, date_sent, provider_code, from_recipient_id)
select m.message_id, m.template_id, isnull(m.created,m.date_of_processing), m.date_of_processing, m.date_of_sending, m.provider, (select recipient_id from Wolf.notif.recipient where address='dev@mycompany.com')
from OLDDB.notif.message m

insert into Wolf.notif.message_recipient (message_id, recipient_id,type_code)
select mr.message_id, r.recipient_id, 'to'
from OLDDB.notif.recipient mr
inner join Wolf.notif.recipient r on r.address=mr.recipient_address

insert into Wolf.notif.generated_message (message_id, body, subject)
select m.message_id, m.generated_body, m.generated_subject from OLDDB.notif.message m 

