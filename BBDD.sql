-- Crear la base de datos
CREATE DATABASE pruebaITS;
GO

USE pruebaITS;
GO

-- Crear la tabla Libros
CREATE TABLE Libros (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Titulo NVARCHAR(255) NOT NULL,
    Autor NVARCHAR(255) NOT NULL,
    Genero NVARCHAR(100) NOT NULL
);
GO

-- Insertar datos en la tabla Libros
INSERT INTO Libros (Titulo, Autor, Genero)
VALUES
('Cien Años de Soledad', 'Gabriel García Márquez', 'Ficción'),
('Don Quijote de la Mancha', 'Miguel de Cervantes', 'Clásico'),
('1984', 'George Orwell', 'Distopía'),
('La Casa de los Espíritus', 'Isabel Allende', 'Realismo Mágico'),
('Matar a un Ruiseñor', 'Harper Lee', 'Drama'),
('El Gran Gatsby', 'F. Scott Fitzgerald', 'Ficción'),
('Cumbres Borrascosas', 'Emily Brontë', 'Romántico');
GO

-- Crear la tabla Copias
CREATE TABLE Copias (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    LibroId INT NOT NULL,
    Disponible BIT DEFAULT 1,
    FOREIGN KEY (LibroId) REFERENCES Libros(Id) ON DELETE CASCADE
);
GO

-- Insertar datos en la tabla Copias
INSERT INTO Copias (LibroId, Disponible)
VALUES
(1, 0),  -- Copia disponible para "Cien Años de Soledad"
(1, 0),  -- Copia no disponible para "Cien Años de Soledad"
(2, 0),  -- Copia disponible para "Don Quijote de la Mancha"
(2, 0),  -- Copia disponible para "Don Quijote de la Mancha"
(3, 1),  -- Copia disponible para "1984"
(4, 1),  -- Copia no disponible para "La Casa de los Espíritus"
(5, 1);  -- Copia disponible para "Matar a un Ruiseñor"
GO

-- Crear la tabla Prestamos
CREATE TABLE Prestamos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CopiaId INT NOT NULL,
    CodigoFiscal NVARCHAR(50) NOT NULL,
    FechaPrestamo DATE NOT NULL,
    FechaDevolucion DATE NOT NULL,
    FOREIGN KEY (CopiaId) REFERENCES Copias(Id) ON DELETE CA1SCADE
);
GO

-- Insertar datos en la tabla Prestamos
INSERT INTO Prestamos (CopiaId, CodigoFiscal, FechaPrestamo, FechaDevolucion)
VALUES
(1, '111222333', '2025-01-15', '2025-01-22'),  -- Préstamo vencido (vencido)
(2, '147258369', '2025-02-05', '2025-02-12'),  -- Préstamo vencido (vencido)
(3, '123456789', '2025-02-10', '2025-02-17'),  -- Préstamo vencido (vencido)
(4, '159753862', '2025-02-15', '2025-02-22'),  -- Préstamo en fecha
(5, '258741963', '2025-02-17', '2025-02-24'),  -- Préstamo en fecha
(6, '164761438', '2025-02-19', '2025-02-26');  -- Préstamo vence en 3 dias
GO