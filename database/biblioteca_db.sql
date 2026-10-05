-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Generation Time: Oct 05, 2026 at 11:43 PM
-- Server version: 10.4.32-MariaDB
-- PHP Version: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `biblioteca_db`
--

-- --------------------------------------------------------

--
-- Table structure for table `afiliado`
--

CREATE TABLE `afiliado` (
  `id` int(11) NOT NULL,
  `nombre` varchar(100) NOT NULL,
  `apellido` varchar(100) NOT NULL,
  `email` varchar(150) DEFAULT NULL,
  `telefono` varchar(50) DEFAULT NULL,
  `fecha_alta` date NOT NULL,
  `estado` tinyint(4) NOT NULL DEFAULT 1,
  `sancionado` tinyint(4) NOT NULL DEFAULT 0,
  `activo` tinyint(4) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `autor`
--

CREATE TABLE `autor` (
  `id` int(11) NOT NULL,
  `nombre` varchar(100) NOT NULL,
  `apellido` varchar(100) NOT NULL,
  `nacionalidad` varchar(100) DEFAULT NULL,
  `biografia` varchar(1000) DEFAULT NULL,
  `foto` longtext DEFAULT NULL,
  `activo` tinyint(4) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `ejemplar`
--

CREATE TABLE `ejemplar` (
  `id` int(11) NOT NULL,
  `id_libro` int(11) NOT NULL,
  `codigo_inventario` int(11) NOT NULL,
  `estado` enum('Disponible','Prestado') NOT NULL DEFAULT 'Disponible',
  `activo` tinyint(4) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `genero`
--

CREATE TABLE `genero` (
  `id` int(11) NOT NULL,
  `nombre` varchar(100) NOT NULL,
  `activo` tinyint(4) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `libro`
--

CREATE TABLE `libro` (
  `id` int(11) NOT NULL,
  `titulo` varchar(255) NOT NULL,
  `id_autor` int(11) NOT NULL,
  `isbn` varchar(20) DEFAULT NULL,
  `paginas` int(11) DEFAULT NULL,
  `anio_publicacion` int(11) DEFAULT NULL,
  `portada` longtext DEFAULT NULL,
  `editorial` varchar(100) DEFAULT NULL,
  `activo` tinyint(4) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `libro_genero`
--

CREATE TABLE `libro_genero` (
  `id_libro` int(11) NOT NULL,
  `id_genero` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `pago`
--

CREATE TABLE `pago` (
  `id` int(11) NOT NULL,
  `id_afiliado` int(11) NOT NULL,
  `creado_por_user_id` int(11) NOT NULL,
  `anulado_por_user_id` int(11) DEFAULT NULL,
  `fecha_pago` date NOT NULL,
  `monto` decimal(10,2) NOT NULL,
  `concepto` varchar(255) NOT NULL,
  `saldado` tinyint(4) NOT NULL DEFAULT 1,
  `activo` tinyint(4) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `prestamo`
--

CREATE TABLE `prestamo` (
  `id` int(11) NOT NULL,
  `id_afiliado` int(11) NOT NULL,
  `id_ejemplar` int(11) NOT NULL,
  `creado_por_user_id` int(11) NOT NULL,
  `anulado_por_user_id` int(11) DEFAULT NULL,
  `fecha_prestamo` date NOT NULL,
  `fecha_devolucion_pactada` date NOT NULL,
  `fecha_devolucion_real` date DEFAULT NULL,
  `estado` varchar(50) NOT NULL,
  `activo` tinyint(4) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `usuario`
--

CREATE TABLE `usuario` (
  `id` int(11) NOT NULL,
  `nombre_usuario` varchar(50) NOT NULL,
  `nombre` varchar(100) NOT NULL,
  `apellido` varchar(100) NOT NULL,
  `email` varchar(150) NOT NULL,
  `password` varchar(255) NOT NULL,
  `avatar` longtext DEFAULT NULL,
  `rol` int(11) NOT NULL,
  `activo` tinyint(4) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Indexes for dumped tables
--

--
-- Indexes for table `afiliado`
--
ALTER TABLE `afiliado`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `autor`
--
ALTER TABLE `autor`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `ejemplar`
--
ALTER TABLE `ejemplar`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `codigo_inventario` (`codigo_inventario`),
  ADD KEY `fk_ejemplar_libro` (`id_libro`);

--
-- Indexes for table `genero`
--
ALTER TABLE `genero`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `libro`
--
ALTER TABLE `libro`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_libro_autor` (`id_autor`);

--
-- Indexes for table `libro_genero`
--
ALTER TABLE `libro_genero`
  ADD PRIMARY KEY (`id_libro`,`id_genero`),
  ADD KEY `fk_libro_genero_genero` (`id_genero`);

--
-- Indexes for table `pago`
--
ALTER TABLE `pago`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_pago_afiliado` (`id_afiliado`),
  ADD KEY `fk_pago_usuario_creador` (`creado_por_user_id`),
  ADD KEY `fk_pago_usuario_anulador` (`anulado_por_user_id`);

--
-- Indexes for table `prestamo`
--
ALTER TABLE `prestamo`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_prestamo_afiliado` (`id_afiliado`),
  ADD KEY `fk_prestamo_ejemplar` (`id_ejemplar`),
  ADD KEY `fk_prestamo_usuario_creador` (`creado_por_user_id`),
  ADD KEY `fk_prestamo_usuario_anulador` (`anulado_por_user_id`);

--
-- Indexes for table `usuario`
--
ALTER TABLE `usuario`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `nombre_usuario` (`nombre_usuario`),
  ADD UNIQUE KEY `email` (`email`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `afiliado`
--
ALTER TABLE `afiliado`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `autor`
--
ALTER TABLE `autor`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `ejemplar`
--
ALTER TABLE `ejemplar`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `genero`
--
ALTER TABLE `genero`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `libro`
--
ALTER TABLE `libro`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `pago`
--
ALTER TABLE `pago`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `prestamo`
--
ALTER TABLE `prestamo`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `usuario`
--
ALTER TABLE `usuario`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `ejemplar`
--
ALTER TABLE `ejemplar`
  ADD CONSTRAINT `fk_ejemplar_libro` FOREIGN KEY (`id_libro`) REFERENCES `libro` (`id`) ON UPDATE CASCADE;

--
-- Constraints for table `libro`
--
ALTER TABLE `libro`
  ADD CONSTRAINT `fk_libro_autor` FOREIGN KEY (`id_autor`) REFERENCES `autor` (`id`) ON UPDATE CASCADE;

--
-- Constraints for table `libro_genero`
--
ALTER TABLE `libro_genero`
  ADD CONSTRAINT `fk_libro_genero_genero` FOREIGN KEY (`id_genero`) REFERENCES `genero` (`id`) ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_libro_genero_libro` FOREIGN KEY (`id_libro`) REFERENCES `libro` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `pago`
--
ALTER TABLE `pago`
  ADD CONSTRAINT `fk_pago_afiliado` FOREIGN KEY (`id_afiliado`) REFERENCES `afiliado` (`id`) ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_pago_usuario_anulador` FOREIGN KEY (`anulado_por_user_id`) REFERENCES `usuario` (`id`) ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_pago_usuario_creador` FOREIGN KEY (`creado_por_user_id`) REFERENCES `usuario` (`id`) ON UPDATE CASCADE;

--
-- Constraints for table `prestamo`
--
ALTER TABLE `prestamo`
  ADD CONSTRAINT `fk_prestamo_afiliado` FOREIGN KEY (`id_afiliado`) REFERENCES `afiliado` (`id`) ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_prestamo_ejemplar` FOREIGN KEY (`id_ejemplar`) REFERENCES `ejemplar` (`id`) ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_prestamo_usuario_anulador` FOREIGN KEY (`anulado_por_user_id`) REFERENCES `usuario` (`id`) ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_prestamo_usuario_creador` FOREIGN KEY (`creado_por_user_id`) REFERENCES `usuario` (`id`) ON UPDATE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
