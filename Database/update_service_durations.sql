-- Обновление длительности услуг (если в БД стоят нули)
-- Запустите этот скрипт, если видите "0 мин" в таблице услуг

UPDATE services SET duration_minutes = 180 WHERE service_name = 'Костюм Наруто';
UPDATE services SET duration_minutes = 90 WHERE service_name = 'Макияж вампира';
UPDATE services SET duration_minutes = 67 WHERE service_name = 'Киберпанк-маска';
UPDATE services SET duration_minutes = 120 WHERE service_name = 'Парик аниме-персонажа';
UPDATE services SET duration_minutes = 60 WHERE service_name = 'Фотосессия в костюме';
UPDATE services SET duration_minutes = 45 WHERE service_name = 'Световой меч';
UPDATE services SET duration_minutes = 240 WHERE service_name = 'Костюм Деда Мороза';
UPDATE services SET duration_minutes = 150 WHERE service_name = 'Кастомизация косплея';
UPDATE services SET duration_minutes = 200 WHERE service_name = 'Костюм детектива';
UPDATE services SET duration_minutes = 30 WHERE service_name = 'Консультация по образу';

-- Проверка результата
SELECT service_name, duration_minutes, price 
FROM services 
ORDER BY service_name;
