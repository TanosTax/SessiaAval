-- База данных для лавки "Матье"
-- СУБД: PostgreSQL
-- Нормализация: 3НФ

-- Таблица ролей пользователей
CREATE TABLE roles (
    role_id SERIAL PRIMARY KEY,
    role_name VARCHAR(50) NOT NULL UNIQUE,
    description TEXT
);

-- Таблица пользователей
CREATE TABLE users (
    user_id SERIAL PRIMARY KEY,
    role_id INTEGER NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    phone VARCHAR(20),
    balance DECIMAL(10, 2) DEFAULT 0.00,
    registration_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_modified TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (role_id) REFERENCES roles(role_id)
);

-- Таблица коллекций
CREATE TABLE collections (
    collection_id SERIAL PRIMARY KEY,
    collection_name VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    last_modified TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Таблица категорий услуг
CREATE TABLE service_categories (
    category_id SERIAL PRIMARY KEY,
    category_name VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    last_modified TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Таблица мастеров
CREATE TABLE masters (
    master_id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL UNIQUE,
    qualification_level INTEGER DEFAULT 1,
    specialization TEXT,
    hire_date DATE NOT NULL,
    qualification_request_pending BOOLEAN DEFAULT FALSE,
    last_modified TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id)
);

-- Таблица услуг
CREATE TABLE services (
    service_id SERIAL PRIMARY KEY,
    service_name VARCHAR(200) NOT NULL,
    description TEXT,
    price DECIMAL(10, 2) NOT NULL,
    duration_minutes INTEGER NOT NULL,
    category_id INTEGER NOT NULL,
    collection_id INTEGER,
    is_active BOOLEAN DEFAULT TRUE,
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_modified TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (category_id) REFERENCES service_categories(category_id),
    FOREIGN KEY (collection_id) REFERENCES collections(collection_id)
);

-- Таблица связи мастеров и услуг
CREATE TABLE master_services (
    master_service_id SERIAL PRIMARY KEY,
    master_id INTEGER NOT NULL,
    service_id INTEGER NOT NULL,
    assigned_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_modified TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (master_id) REFERENCES masters(master_id),
    FOREIGN KEY (service_id) REFERENCES services(service_id),
    UNIQUE(master_id, service_id)
);

-- Таблица записей на услуги
CREATE TABLE appointments (
    appointment_id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL,
    master_id INTEGER NOT NULL,
    service_id INTEGER NOT NULL,
    appointment_date TIMESTAMP NOT NULL,
    queue_number INTEGER NOT NULL,
    status VARCHAR(20) DEFAULT 'pending',
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_modified TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id),
    FOREIGN KEY (master_id) REFERENCES masters(master_id),
    FOREIGN KEY (service_id) REFERENCES services(service_id)
);

-- Таблица транзакций баланса
CREATE TABLE balance_transactions (
    transaction_id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL,
    amount DECIMAL(10, 2) NOT NULL,
    transaction_type VARCHAR(20) NOT NULL,
    card_last_digits VARCHAR(4),
    transaction_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id)
);

-- Таблица отзывов
CREATE TABLE reviews (
    review_id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL,
    service_id INTEGER,
    master_id INTEGER,
    rating INTEGER CHECK (rating >= 1 AND rating <= 5),
    comment TEXT,
    review_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_modified TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id),
    FOREIGN KEY (service_id) REFERENCES services(service_id),
    FOREIGN KEY (master_id) REFERENCES masters(master_id)
);

-- Индексы для оптимизации
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_role ON users(role_id);
CREATE INDEX idx_services_category ON services(category_id);
CREATE INDEX idx_services_collection ON services(collection_id);
CREATE INDEX idx_appointments_user ON appointments(user_id);
CREATE INDEX idx_appointments_master ON appointments(master_id);
CREATE INDEX idx_appointments_date ON appointments(appointment_date);
CREATE INDEX idx_reviews_service ON reviews(service_id);
CREATE INDEX idx_reviews_master ON reviews(master_id);

-- Триггеры для автоматического обновления last_modified
CREATE OR REPLACE FUNCTION update_last_modified()
RETURNS TRIGGER AS $$
BEGIN
    NEW.last_modified = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER update_users_last_modified
    BEFORE UPDATE ON users
    FOR EACH ROW
    EXECUTE FUNCTION update_last_modified();

CREATE TRIGGER update_services_last_modified
    BEFORE UPDATE ON services
    FOR EACH ROW
    EXECUTE FUNCTION update_last_modified();

CREATE TRIGGER update_masters_last_modified
    BEFORE UPDATE ON masters
    FOR EACH ROW
    EXECUTE FUNCTION update_last_modified();

CREATE TRIGGER update_collections_last_modified
    BEFORE UPDATE ON collections
    FOR EACH ROW
    EXECUTE FUNCTION update_last_modified();

CREATE TRIGGER update_service_categories_last_modified
    BEFORE UPDATE ON service_categories
    FOR EACH ROW
    EXECUTE FUNCTION update_last_modified();

CREATE TRIGGER update_master_services_last_modified
    BEFORE UPDATE ON master_services
    FOR EACH ROW
    EXECUTE FUNCTION update_last_modified();

CREATE TRIGGER update_appointments_last_modified
    BEFORE UPDATE ON appointments
    FOR EACH ROW
    EXECUTE FUNCTION update_last_modified();

CREATE TRIGGER update_reviews_last_modified
    BEFORE UPDATE ON reviews
    FOR EACH ROW
    EXECUTE FUNCTION update_last_modified();
