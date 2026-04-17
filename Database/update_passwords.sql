-- Обновление паролей всех тестовых пользователей на "123"
-- Запустите этот скрипт в PostgreSQL, если не хотите пересоздавать базу

UPDATE users SET password_hash = '123' 
WHERE email IN (
    'admin@matye.ru',
    'moderator@matye.ru',
    'master1@matye.ru',
    'master2@matye.ru',
    'master3@matye.ru',
    'user1@mail.ru',
    'user2@mail.ru',
    'user3@mail.ru',
    'user4@mail.ru',
    'user5@mail.ru'
);

-- Проверка результата
SELECT email, password_hash, first_name, last_name 
FROM users 
ORDER BY email;
