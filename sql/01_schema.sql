-- Script de creación de tablas · schema prueba_jperez
-- Charset utf8 / collation utf8_bin (comparaciones sensibles a mayúsculas)

CREATE TABLE IF NOT EXISTS producto (
  id          INT AUTO_INCREMENT PRIMARY KEY,
  descripcion VARCHAR(200) NOT NULL,
  cantidad    INT NOT NULL DEFAULT 0,
  valor_venta DECIMAL(12,2) NOT NULL,
  estado      TINYINT(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;

CREATE TABLE IF NOT EXISTS cliente (
  id        INT AUTO_INCREMENT PRIMARY KEY,
  nombre    VARCHAR(150) NOT NULL,
  rut       VARCHAR(12)  NOT NULL,
  direccion VARCHAR(250) NOT NULL,
  correo    VARCHAR(150) NOT NULL,
  estado    TINYINT(1) NOT NULL DEFAULT 1,
  UNIQUE KEY uq_cliente_rut (rut)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;

CREATE TABLE IF NOT EXISTS pedido (
  id            INT AUTO_INCREMENT PRIMARY KEY,
  cliente_id    INT NOT NULL,
  fecha_pedido  DATETIME NOT NULL,
  observaciones VARCHAR(500) NULL,
  CONSTRAINT fk_pedido_cliente FOREIGN KEY (cliente_id) REFERENCES cliente(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;

CREATE TABLE IF NOT EXISTS pedido_detalle (
  id             INT AUTO_INCREMENT PRIMARY KEY,
  pedido_id      INT NOT NULL,
  producto_id    INT NOT NULL,
  cantidad       INT NOT NULL,
  valor_unitario DECIMAL(12,2) NOT NULL,
  CONSTRAINT fk_det_pedido   FOREIGN KEY (pedido_id)   REFERENCES pedido(id),
  CONSTRAINT fk_det_producto FOREIGN KEY (producto_id) REFERENCES producto(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;

CREATE TABLE IF NOT EXISTS log (
  id          INT AUTO_INCREMENT PRIMARY KEY,
  descripcion VARCHAR(1000) NOT NULL,
  fecha       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
