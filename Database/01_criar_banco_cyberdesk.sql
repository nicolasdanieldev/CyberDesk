CREATE DATABASE IF NOT EXISTS cyberdesk
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE cyberdesk;

CREATE TABLE IF NOT EXISTS ativos (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nome VARCHAR(120) NOT NULL,
    endereco_ip VARCHAR(45) NOT NULL,
    setor VARCHAR(100) NOT NULL,
    responsavel VARCHAR(120) NOT NULL,
    CONSTRAINT uk_ativos_ip UNIQUE (endereco_ip)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS incidentes (
    id INT PRIMARY KEY AUTO_INCREMENT,
    ativo_id INT NOT NULL,
    titulo VARCHAR(150) NOT NULL,
    descricao TEXT NOT NULL,
    impacto TINYINT NOT NULL,
    probabilidade TINYINT NOT NULL,
    status TINYINT NOT NULL DEFAULT 0,
    data_abertura DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_incidentes_ativos
        FOREIGN KEY (ativo_id) REFERENCES ativos(id)
        ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT ck_incidentes_impacto CHECK (impacto BETWEEN 1 AND 5),
    CONSTRAINT ck_incidentes_probabilidade CHECK (probabilidade BETWEEN 1 AND 5),
    CONSTRAINT ck_incidentes_status CHECK (status IN (0,1,2)),
    INDEX idx_incidentes_ativo (ativo_id),
    INDEX idx_incidentes_status (status)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS historico_incidentes (
    id INT PRIMARY KEY AUTO_INCREMENT,
    incidente_id INT NOT NULL,
    status_anterior TINYINT NOT NULL,
    status_novo TINYINT NOT NULL,
    observacao VARCHAR(500) NOT NULL,
    data_alteracao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_historico_incidente
        FOREIGN KEY (incidente_id) REFERENCES incidentes(id)
        ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT ck_historico_anterior CHECK (status_anterior IN (0,1,2)),
    CONSTRAINT ck_historico_novo CHECK (status_novo IN (0,1,2)),
    INDEX idx_historico_incidente (incidente_id)
) ENGINE=InnoDB;

-- Consultas demonstrativas para o professor

SELECT * 
FROM ativos;

SELECT 
   i.id, 
   i.titulo, 
   a.nome AS equipamento, 
   i.status,
   (i.impacto * i.probabilidade) AS pontuacao_risco
   FROM incidentes i 
   INNER JOIN ativos a 
      ON a.id = i.ativo_id;
      
SELECT * 
FROM historico_incidentes 
ORDER BY data_alteracao DESC;
