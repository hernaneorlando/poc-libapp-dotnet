insert into Authors (Id, FirstName, LastName, DateOfBirth, CreatedAt, UpdatedAt) values
(newid(), 'Aldous', 'Huxley', convert(datetime,'Jul 26 1894'), getdate(), getdate()),
(newid(), 'Jostein', 'Gaarder', convert(datetime,'Aug 08 1952'), getdate(), getdate()),
(newid(), 'George', 'Orwell', convert(datetime,'Jul 25 1903'), getdate(), getdate());