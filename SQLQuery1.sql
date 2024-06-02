use diningSystem


select * from Orders

drop table Orders

create table Orders (
	orderId int primary key identity,
	UserId nvarchar(450),
	OrderType varchar(50),
	DineInDate Date,
	DineInTime Time,
	NumberOfPersons int,
	DeliveryAddress nvarchar(450),
	CardNumber varchar(20),
	ExpirationDate varchar(5),
	CVV varchar(3),
	order_status varchar(10),
	r_id int,
	foreign key (UserId) references AspNetUsers(Id),
	foreign key (r_id) references restaurant(r_id)
);


SELECT o.orderId, o.UserId, u.FirstName + ' ' + u.LastName AS UserName, o.OrderType, o.DineInDate, o.DineInTime, o.NumberOfPersons, o.DeliveryAddress, o.CardNumber, o.ExpirationDate, o.CVV, o.order_status, o.r_id
                    FROM Orders o
                    JOIN Restaurant r ON o.r_id = r.r_id
                    JOIN AspNetUsers u ON o.UserId = u.Id
                    WHERE r.r_admin = u.Id



select * from CartItems

select * from Orders
insert into Orders values ('2cb8c16c-f5df-4a33-8012-8bee1c819471', 'DineIn', '2024-05-31', '03:00:00.0000000', 4, 'CE17 Airport, Malir Halt', '4242 4242 4242 4242', '01/25', 123, 'pending', 4)
select * from AspNetUsers


SELECT o.orderId, o.UserId, u.UserName, o.OrderType, o.DineInDate, o.DineInTime, o.NumberOfPersons, o.DeliveryAddress, o.CardNumber, o.ExpirationDate, o.CVV, o.order_status, r.r_name
                    FROM Orders o
                    JOIN Restaurant r ON o.r_id = r.r_id
                    JOIN AspNetUsers u ON o.UserId = u.Id
                    WHERE r.r_admin = u.Id;

select o.orderId, o.UserId, o.OrderType, o.DineInDate, o.DineInTime, o.NumberOfPersons, o.DeliveryAddress, o.CardNumber, o.ExpirationDate, o.CVV, o.order_status, r.r_name
				from orders o
				join restaurant r on o.r_id = r.r_id
				join AspNetUsers u on o.UserId = u.Id
				where r.r_admin = u.Id;





