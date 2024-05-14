create table restaurant(
	r_id int primary key identity,
	r_name nvarchar(100) not null,
	r_description nvarchar(150),
	r_admin nvarchar(450),
	FOREIGN KEY (r_admin) REFERENCES AspNetUsers(Id)
);


insert into restaurant(r_name, r_description, r_admin)
values('CAFE HAWAII', 'Cafe Hawaii Restaurant blends the flavors of the islands with international cuisine in Karachi.', 'd6ce7ed4-4dae-4aab-84e3-8c9c31f2e3ce');

select * from restaurant


create table restaurantMenu(
	menu_id int identity,
	menu_item nvarchar(50),
	menu_description nvarchar(200),
	menu_item_price int not null,
	r_id int,
	foreign key (r_id) references restaurant(r_id)
);

drop table restaurantMenu

select * from restaurantMenu

insert into restaurantMenu values('Cappuccino Coffee', 'Cappuccino: Rich espresso topped with frothy milk, a delightful blend of bold and creamy flavors.', 350, 3);

select * from restaurantMenu where r_id = 3;

select * from AspNetUsers

delete from AspNetUsers where FirstName = 'm'