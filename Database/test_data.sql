-- Тестовые данные для базы данных лавки "Матье"
-- 10 записей для каждой таблицы

-- Роли (4 роли)
INSERT INTO roles (role_name, description) VALUES
('Пользователь', 'Обычный клиент лавки'),
('Мастер', 'Специалист по оказанию услуг'),
('Модератор', 'Управление услугами и мастерами'),
('Администратор', 'Полный доступ к системе')
ON CONFLICT DO NOTHING;

-- Пользователи (10 записей)
INSERT INTO users (role_id, email, password_hash, first_name, last_name, phone, balance) VALUES
((SELECT role_id FROM roles WHERE role_name = 'Администратор'), 'admin@matye.ru', '123', 'Иван', 'Петров', '+79001234567', 0.00),
((SELECT role_id FROM roles WHERE role_name = 'Модератор'), 'moderator@matye.ru', '123', 'Мария', 'Сидорова', '+79001234568', 0.00),
((SELECT role_id FROM roles WHERE role_name = 'Мастер'), 'master1@matye.ru', '123', 'Алексей', 'Кузнецов', '+79001234569', 0.00),
((SELECT role_id FROM roles WHERE role_name = 'Мастер'), 'master2@matye.ru', '123', 'Елена', 'Смирнова', '+79001234570', 0.00),
((SELECT role_id FROM roles WHERE role_name = 'Мастер'), 'master3@matye.ru', '123', 'Дмитрий', 'Волков', '+79001234571', 0.00),
((SELECT role_id FROM roles WHERE role_name = 'Пользователь'), 'user1@mail.ru', '123', 'Анна', 'Иванова', '+79001234572', 5000.00),
((SELECT role_id FROM roles WHERE role_name = 'Пользователь'), 'user2@mail.ru', '123', 'Сергей', 'Морозов', '+79001234573', 3000.00),
((SELECT role_id FROM roles WHERE role_name = 'Пользователь'), 'user3@mail.ru', '123', 'Ольга', 'Новикова', '+79001234574', 7500.00),
((SELECT role_id FROM roles WHERE role_name = 'Пользователь'), 'user4@mail.ru', '123', 'Павел', 'Соколов', '+79001234575', 2000.00),
((SELECT role_id FROM roles WHERE role_name = 'Пользователь'), 'user5@mail.ru', '123', 'Татьяна', 'Лебедева', '+79001234576', 4500.00)
ON CONFLICT (email) DO NOTHING;

-- Коллекции (5 коллекций)
INSERT INTO collections (collection_name, description) VALUES
('Аниме', 'Косплей персонажей из аниме и манги'),
('Новый год', 'Праздничные костюмы и аксессуары'),
('Хэллоуин', 'Костюмы для Хэллоуина'),
('Киберпанк', 'Футуристические костюмы в стиле киберпанк'),
('Нуар', 'Костюмы в стиле нуар и детектива')
ON CONFLICT DO NOTHING;

-- Категории услуг (10 категорий)
INSERT INTO service_categories (category_name, description) VALUES
('Создание костюма', 'Полное изготовление костюма с нуля'),
('Кастомизация', 'Доработка и улучшение существующего костюма'),
('Аксессуары', 'Изготовление аксессуаров и реквизита'),
('Грим и макияж', 'Профессиональный грим для косплея'),
('Парики', 'Стилизация и создание париков'),
('Оружие и реквизит', 'Изготовление безопасного реквизита'),
('Фотосессия', 'Профессиональная фотосессия в костюме'),
('Консультация', 'Консультация по созданию образа'),
('Ремонт костюма', 'Ремонт и восстановление костюмов'),
('Аренда костюма', 'Аренда готовых костюмов')
ON CONFLICT DO NOTHING;

-- Мастера (3 мастера из пользователей)
INSERT INTO masters (user_id, qualification_level, specialization, hire_date, qualification_request_pending) 
SELECT user_id, 3, 'Создание аниме-костюмов, работа с тканями', '2022-01-15', FALSE 
FROM users WHERE email = 'master1@matye.ru'
ON CONFLICT DO NOTHING;

INSERT INTO masters (user_id, qualification_level, specialization, hire_date, qualification_request_pending) 
SELECT user_id, 2, 'Грим и макияж, спецэффекты', '2022-06-20', FALSE 
FROM users WHERE email = 'master2@matye.ru'
ON CONFLICT DO NOTHING;

INSERT INTO masters (user_id, qualification_level, specialization, hire_date, qualification_request_pending) 
SELECT user_id, 1, 'Изготовление реквизита и оружия', '2023-03-10', TRUE 
FROM users WHERE email = 'master3@matye.ru'
ON CONFLICT DO NOTHING;

-- Услуги (10 услуг)
INSERT INTO services (service_name, description, price, duration_minutes, category_id, collection_id, is_active) VALUES
('Костюм Наруто', 'Полный костюм Наруто Узумаки с аксессуарами', 15000.00, 720, 
 (SELECT category_id FROM service_categories WHERE category_name = 'Создание костюма'), 
 (SELECT collection_id FROM collections WHERE collection_name = 'Аниме'), TRUE),
('Костюм Деда Мороза', 'Классический костюм Деда Мороза', 8000.00, 360, 
 (SELECT category_id FROM service_categories WHERE category_name = 'Создание костюма'), 
 (SELECT collection_id FROM collections WHERE collection_name = 'Новый год'), TRUE),
('Макияж вампира', 'Профессиональный грим вампира для Хэллоуина', 2500.00, 90, 
 (SELECT category_id FROM service_categories WHERE category_name = 'Грим и макияж'), 
 (SELECT collection_id FROM collections WHERE collection_name = 'Хэллоуин'), TRUE),
('Киберпанк-маска', 'LED-маска в стиле киберпанк', 5000.00, 240, 
 (SELECT category_id FROM service_categories WHERE category_name = 'Аксессуары'), 
 (SELECT collection_id FROM collections WHERE collection_name = 'Киберпанк'), TRUE),
('Костюм детектива', 'Классический костюм детектива в стиле нуар', 12000.00, 480, 
 (SELECT category_id FROM service_categories WHERE category_name = 'Создание костюма'), 
 (SELECT collection_id FROM collections WHERE collection_name = 'Нуар'), TRUE),
('Кастомизация косплея', 'Доработка существующего костюма', 3000.00, 180, 
 (SELECT category_id FROM service_categories WHERE category_name = 'Кастомизация'), NULL, TRUE),
('Парик аниме-персонажа', 'Стилизация парика под аниме-персонажа', 4000.00, 120, 
 (SELECT category_id FROM service_categories WHERE category_name = 'Парики'), 
 (SELECT collection_id FROM collections WHERE collection_name = 'Аниме'), TRUE),
('Световой меч', 'Изготовление светового меча с подсветкой', 6000.00, 300, 
 (SELECT category_id FROM service_categories WHERE category_name = 'Оружие и реквизит'), NULL, TRUE),
('Фотосессия в костюме', 'Профессиональная фотосессия (2 часа)', 5000.00, 120, 
 (SELECT category_id FROM service_categories WHERE category_name = 'Фотосессия'), NULL, TRUE),
('Консультация по образу', 'Помощь в выборе и создании образа', 1500.00, 60, 
 (SELECT category_id FROM service_categories WHERE category_name = 'Консультация'), NULL, TRUE)
ON CONFLICT DO NOTHING;

-- Связь мастеров и услуг (10 связей)
INSERT INTO master_services (master_id, service_id) 
SELECT m.master_id, s.service_id 
FROM masters m, services s 
WHERE m.user_id = (SELECT user_id FROM users WHERE email = 'master1@matye.ru')
AND s.service_name IN ('Костюм Наруто', 'Костюм Деда Мороза', 'Парик аниме-персонажа', 'Консультация по образу')
ON CONFLICT DO NOTHING;

INSERT INTO master_services (master_id, service_id) 
SELECT m.master_id, s.service_id 
FROM masters m, services s 
WHERE m.user_id = (SELECT user_id FROM users WHERE email = 'master2@matye.ru')
AND s.service_name IN ('Макияж вампира', 'Фотосессия в костюме', 'Кастомизация косплея')
ON CONFLICT DO NOTHING;

INSERT INTO master_services (master_id, service_id) 
SELECT m.master_id, s.service_id 
FROM masters m, services s 
WHERE m.user_id = (SELECT user_id FROM users WHERE email = 'master3@matye.ru')
AND s.service_name IN ('Киберпанк-маска', 'Световой меч', 'Костюм детектива')
ON CONFLICT DO NOTHING;

-- Записи на услуги (10 записей)
INSERT INTO appointments (user_id, master_id, service_id, appointment_date, queue_number, status) 
SELECT 
    (SELECT user_id FROM users WHERE email = 'user1@mail.ru'),
    (SELECT master_id FROM masters WHERE user_id = (SELECT user_id FROM users WHERE email = 'master1@matye.ru')),
    (SELECT service_id FROM services WHERE service_name = 'Костюм Наруто'),
    '2024-05-15 10:00:00', 1, 'confirmed'
ON CONFLICT DO NOTHING;

INSERT INTO appointments (user_id, master_id, service_id, appointment_date, queue_number, status) 
SELECT 
    (SELECT user_id FROM users WHERE email = 'user2@mail.ru'),
    (SELECT master_id FROM masters WHERE user_id = (SELECT user_id FROM users WHERE email = 'master2@matye.ru')),
    (SELECT service_id FROM services WHERE service_name = 'Макияж вампира'),
    '2024-05-16 14:00:00', 2, 'confirmed'
ON CONFLICT DO NOTHING;

INSERT INTO appointments (user_id, master_id, service_id, appointment_date, queue_number, status) 
SELECT 
    (SELECT user_id FROM users WHERE email = 'user3@mail.ru'),
    (SELECT master_id FROM masters WHERE user_id = (SELECT user_id FROM users WHERE email = 'master3@matye.ru')),
    (SELECT service_id FROM services WHERE service_name = 'Киберпанк-маска'),
    '2024-05-17 11:00:00', 3, 'pending'
ON CONFLICT DO NOTHING;

INSERT INTO appointments (user_id, master_id, service_id, appointment_date, queue_number, status) 
SELECT 
    (SELECT user_id FROM users WHERE email = 'user4@mail.ru'),
    (SELECT master_id FROM masters WHERE user_id = (SELECT user_id FROM users WHERE email = 'master1@matye.ru')),
    (SELECT service_id FROM services WHERE service_name = 'Парик аниме-персонажа'),
    '2024-05-18 15:00:00', 4, 'confirmed'
ON CONFLICT DO NOTHING;

INSERT INTO appointments (user_id, master_id, service_id, appointment_date, queue_number, status) 
SELECT 
    (SELECT user_id FROM users WHERE email = 'user5@mail.ru'),
    (SELECT master_id FROM masters WHERE user_id = (SELECT user_id FROM users WHERE email = 'master2@matye.ru')),
    (SELECT service_id FROM services WHERE service_name = 'Фотосессия в костюме'),
    '2024-05-19 10:00:00', 5, 'completed'
ON CONFLICT DO NOTHING;

INSERT INTO appointments (user_id, master_id, service_id, appointment_date, queue_number, status) 
SELECT 
    (SELECT user_id FROM users WHERE email = 'user1@mail.ru'),
    (SELECT master_id FROM masters WHERE user_id = (SELECT user_id FROM users WHERE email = 'master3@matye.ru')),
    (SELECT service_id FROM services WHERE service_name = 'Световой меч'),
    '2024-05-20 13:00:00', 6, 'pending'
ON CONFLICT DO NOTHING;

INSERT INTO appointments (user_id, master_id, service_id, appointment_date, queue_number, status) 
SELECT 
    (SELECT user_id FROM users WHERE email = 'user2@mail.ru'),
    (SELECT master_id FROM masters WHERE user_id = (SELECT user_id FROM users WHERE email = 'master1@matye.ru')),
    (SELECT service_id FROM services WHERE service_name = 'Костюм Деда Мороза'),
    '2024-05-21 09:00:00', 7, 'confirmed'
ON CONFLICT DO NOTHING;

INSERT INTO appointments (user_id, master_id, service_id, appointment_date, queue_number, status) 
SELECT 
    (SELECT user_id FROM users WHERE email = 'user3@mail.ru'),
    (SELECT master_id FROM masters WHERE user_id = (SELECT user_id FROM users WHERE email = 'master2@matye.ru')),
    (SELECT service_id FROM services WHERE service_name = 'Кастомизация косплея'),
    '2024-05-22 16:00:00', 8, 'confirmed'
ON CONFLICT DO NOTHING;

INSERT INTO appointments (user_id, master_id, service_id, appointment_date, queue_number, status) 
SELECT 
    (SELECT user_id FROM users WHERE email = 'user4@mail.ru'),
    (SELECT master_id FROM masters WHERE user_id = (SELECT user_id FROM users WHERE email = 'master3@matye.ru')),
    (SELECT service_id FROM services WHERE service_name = 'Костюм детектива'),
    '2024-05-23 12:00:00', 9, 'pending'
ON CONFLICT DO NOTHING;

INSERT INTO appointments (user_id, master_id, service_id, appointment_date, queue_number, status) 
SELECT 
    (SELECT user_id FROM users WHERE email = 'user5@mail.ru'),
    (SELECT master_id FROM masters WHERE user_id = (SELECT user_id FROM users WHERE email = 'master1@matye.ru')),
    (SELECT service_id FROM services WHERE service_name = 'Консультация по образу'),
    '2024-05-24 14:00:00', 10, 'confirmed'
ON CONFLICT DO NOTHING;

-- Транзакции баланса (10 транзакций)
INSERT INTO balance_transactions (user_id, amount, transaction_type, card_last_digits) 
SELECT user_id, 5000.00, 'deposit', '1234' FROM users WHERE email = 'user1@mail.ru'
ON CONFLICT DO NOTHING;

INSERT INTO balance_transactions (user_id, amount, transaction_type, card_last_digits) 
SELECT user_id, 3000.00, 'deposit', '5678' FROM users WHERE email = 'user2@mail.ru'
ON CONFLICT DO NOTHING;

INSERT INTO balance_transactions (user_id, amount, transaction_type, card_last_digits) 
SELECT user_id, 7500.00, 'deposit', '9012' FROM users WHERE email = 'user3@mail.ru'
ON CONFLICT DO NOTHING;

INSERT INTO balance_transactions (user_id, amount, transaction_type, card_last_digits) 
SELECT user_id, 2000.00, 'deposit', '3456' FROM users WHERE email = 'user4@mail.ru'
ON CONFLICT DO NOTHING;

INSERT INTO balance_transactions (user_id, amount, transaction_type, card_last_digits) 
SELECT user_id, 4500.00, 'deposit', '7890' FROM users WHERE email = 'user5@mail.ru'
ON CONFLICT DO NOTHING;

INSERT INTO balance_transactions (user_id, amount, transaction_type, card_last_digits) 
SELECT user_id, -15000.00, 'payment', NULL FROM users WHERE email = 'user1@mail.ru'
ON CONFLICT DO NOTHING;

INSERT INTO balance_transactions (user_id, amount, transaction_type, card_last_digits) 
SELECT user_id, -2500.00, 'payment', NULL FROM users WHERE email = 'user2@mail.ru'
ON CONFLICT DO NOTHING;

INSERT INTO balance_transactions (user_id, amount, transaction_type, card_last_digits) 
SELECT user_id, -5000.00, 'payment', NULL FROM users WHERE email = 'user3@mail.ru'
ON CONFLICT DO NOTHING;

INSERT INTO balance_transactions (user_id, amount, transaction_type, card_last_digits) 
SELECT user_id, -4000.00, 'payment', NULL FROM users WHERE email = 'user4@mail.ru'
ON CONFLICT DO NOTHING;

INSERT INTO balance_transactions (user_id, amount, transaction_type, card_last_digits) 
SELECT user_id, -5000.00, 'payment', NULL FROM users WHERE email = 'user5@mail.ru'
ON CONFLICT DO NOTHING;

-- Отзывы (10 отзывов)
INSERT INTO reviews (user_id, service_id, master_id, rating, comment) 
SELECT 
    (SELECT user_id FROM users WHERE email = 'user1@mail.ru'),
    (SELECT service_id FROM services WHERE service_name = 'Костюм Наруто'),
    (SELECT master_id FROM masters WHERE user_id = (SELECT user_id FROM users WHERE email = 'master1@matye.ru')),
    5, 'Отличный костюм! Все детали проработаны идеально.'
ON CONFLICT DO NOTHING;

INSERT INTO reviews (user_id, service_id, master_id, rating, comment) 
SELECT 
    (SELECT user_id FROM users WHERE email = 'user2@mail.ru'),
    (SELECT service_id FROM services WHERE service_name = 'Макияж вампира'),
    (SELECT master_id FROM masters WHERE user_id = (SELECT user_id FROM users WHERE email = 'master2@matye.ru')),
    5, 'Потрясающий грим, все гости были в восторге!'
ON CONFLICT DO NOTHING;

INSERT INTO reviews (user_id, service_id, master_id, rating, comment) 
SELECT 
    (SELECT user_id FROM users WHERE email = 'user3@mail.ru'),
    (SELECT service_id FROM services WHERE service_name = 'Киберпанк-маска'),
    (SELECT master_id FROM masters WHERE user_id = (SELECT user_id FROM users WHERE email = 'master3@matye.ru')),
    4, 'Хорошая работа, но хотелось бы больше LED-элементов.'
ON CONFLICT DO NOTHING;

INSERT INTO reviews (user_id, service_id, master_id, rating, comment) 
SELECT 
    (SELECT user_id FROM users WHERE email = 'user4@mail.ru'),
    (SELECT service_id FROM services WHERE service_name = 'Парик аниме-персонажа'),
    (SELECT master_id FROM masters WHERE user_id = (SELECT user_id FROM users WHERE email = 'master1@matye.ru')),
    5, 'Парик выглядит как настоящие волосы!'
ON CONFLICT DO NOTHING;

INSERT INTO reviews (user_id, service_id, master_id, rating, comment) 
SELECT 
    (SELECT user_id FROM users WHERE email = 'user5@mail.ru'),
    (SELECT service_id FROM services WHERE service_name = 'Фотосессия в костюме'),
    (SELECT master_id FROM masters WHERE user_id = (SELECT user_id FROM users WHERE email = 'master2@matye.ru')),
    5, 'Профессиональная фотосессия, отличные кадры.'
ON CONFLICT DO NOTHING;

INSERT INTO reviews (user_id, service_id, master_id, rating, comment) 
SELECT 
    (SELECT user_id FROM users WHERE email = 'user1@mail.ru'),
    NULL,
    (SELECT master_id FROM masters WHERE user_id = (SELECT user_id FROM users WHERE email = 'master1@matye.ru')),
    5, 'Алексей - мастер своего дела, рекомендую!'
ON CONFLICT DO NOTHING;

INSERT INTO reviews (user_id, service_id, master_id, rating, comment) 
SELECT 
    (SELECT user_id FROM users WHERE email = 'user2@mail.ru'),
    NULL,
    (SELECT master_id FROM masters WHERE user_id = (SELECT user_id FROM users WHERE email = 'master2@matye.ru')),
    5, 'Елена очень внимательная и талантливая.'
ON CONFLICT DO NOTHING;

INSERT INTO reviews (user_id, service_id, master_id, rating, comment) 
SELECT 
    (SELECT user_id FROM users WHERE email = 'user3@mail.ru'),
    (SELECT service_id FROM services WHERE service_name = 'Световой меч'),
    (SELECT master_id FROM masters WHERE user_id = (SELECT user_id FROM users WHERE email = 'master3@matye.ru')),
    4, 'Световой меч классный, но немного тяжеловат.'
ON CONFLICT DO NOTHING;

INSERT INTO reviews (user_id, service_id, master_id, rating, comment) 
SELECT 
    (SELECT user_id FROM users WHERE email = 'user4@mail.ru'),
    (SELECT service_id FROM services WHERE service_name = 'Костюм детектива'),
    (SELECT master_id FROM masters WHERE user_id = (SELECT user_id FROM users WHERE email = 'master3@matye.ru')),
    5, 'Отлично доработали мой старый костюм!'
ON CONFLICT DO NOTHING; 

INSERT INTO reviews (user_id, service_id, master_id, rating, comment) 
SELECT 
    (SELECT user_id FROM users WHERE email = 'user5@mail.ru'),
    (SELECT service_id FROM services WHERE service_name = 'Консультация по образу'),
    (SELECT master_id FROM masters WHERE user_id = (SELECT user_id FROM users WHERE email = 'master1@matye.ru')),
    5, 'Костюм детектива превзошел все ожидания!'
ON CONFLICT DO NOTHING;
