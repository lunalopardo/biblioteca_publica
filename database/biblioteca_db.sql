-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 05-11-2026 a las 02:12:27
-- Versión del servidor: 10.4.32-MariaDB
-- Versión de PHP: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de datos: `biblioteca_db`
--

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `afiliado`
--

CREATE TABLE `afiliado` (
  `id` int(11) NOT NULL,
  `nombre` varchar(100) NOT NULL,
  `apellido` varchar(100) NOT NULL,
  `email` varchar(150) DEFAULT NULL,
  `telefono` varchar(50) DEFAULT NULL,
  `fecha_alta` date NOT NULL,
  `activo` tinyint(4) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `afiliado`
--

INSERT INTO `afiliado` (`id`, `nombre`, `apellido`, `email`, `telefono`, `fecha_alta`, `activo`) VALUES
(1, 'Lucía', 'Fernández', 'lucia.fernandez@gmail.com', '2664123401', '2025-09-29', 1),
(2, 'Mateo', 'Gómez', 'mateo.gomez@gmail.com', '2664123402', '2025-11-18', 1),
(3, 'Camila', 'Rodríguez', 'camila.rodriguez@gmail.com', '2664123403', '2026-01-07', 1),
(4, 'Joaquín', 'Pérez', 'joaquin.perez@gmail.com', '2664123404', '2026-01-27', 1),
(5, 'Valentina', 'Sosa', 'valentina.sosa@gmail.com', '2664123405', '2026-04-17', 1),
(6, 'Santiago', 'Díaz', 'santiago.diaz@gmail.com', '2664123406', '2026-06-06', 1),
(7, 'Martina', 'López', 'martina.lopez@gmail.com', '2664123407', '2026-07-06', 1),
(8, 'Tomás', 'Herrera', 'tomas.herrera@gmail.com', '2664123408', '2026-08-05', 1),
(9, 'Julieta', 'Castro', 'julieta.castro@gmail.com', '2664123409', '2026-10-04', 1),
(10, 'Nicolás', 'Romero', 'nicolas.romero@gmail.com', '2664123410', '2026-07-26', 1),
(11, 'Agustina', 'Molina', 'agustina.molina@gmail.com', '2664123411', '2026-10-31', 1),
(12, 'Bruno', 'Acosta', 'bruno.acosta@gmail.com', '2664123412', '2026-10-14', 1),
(13, 'Lucía', 'Morelli', 'lucia.morelli@gmail.com', '2665676543', '2026-11-03', 1);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `autor`
--

CREATE TABLE `autor` (
  `id` int(11) NOT NULL,
  `nombre` varchar(100) NOT NULL,
  `apellido` varchar(100) NOT NULL,
  `biografia` varchar(1000) DEFAULT NULL,
  `foto` longtext DEFAULT NULL,
  `activo` tinyint(4) NOT NULL DEFAULT 1,
  `id_nacionalidad` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `autor`
--

INSERT INTO `autor` (`id`, `nombre`, `apellido`, `biografia`, `foto`, `activo`, `id_nacionalidad`) VALUES
(1, 'Jorge Luis', 'Borges', 'Escritor, poeta y ensayista argentino (1899-1986), considerado una de las figuras centrales de la literatura en lengua española.', '/uploads/autores/2bd2d48f-b414-4197-8572-07d51af7c029.jpg', 1, 1),
(2, 'Julio', 'Cortázar', 'Escritor argentino (1914-1984), maestro del cuento fantástico y autor de Rayuela.', '/uploads/autores/1cae0bc9-5522-43ec-9284-ea058916d048.jpg', 1, 1),
(3, 'Gabriel', 'García Márquez', 'Escritor colombiano (1927-2014), Premio Nobel de Literatura 1982 y referente del realismo mágico.', '/uploads/autores/5d42e7d6-ae0d-45e6-8f5c-e1886212f05a.jpg', 1, 2),
(4, 'Isabel', 'Allende', 'Escritora chilena nacida en 1942, una de las autoras más leídas en lengua española.', '/uploads/autores/76422047-c45a-4ba3-8c8a-7476fc8c5e41.webp', 1, 5),
(5, 'Stephen', 'King', 'Escritor estadounidense nacido en 1947, conocido por sus novelas de terror y suspenso.', '/uploads/autores/d902216c-ecc7-4ab7-a26b-7a9fd100348c.jpg', 1, 3),
(6, 'J. R. R.', 'Tolkien', 'Escritor y filólogo británico (1892-1973), autor de El Hobbit y El Señor de los Anillos.', '/uploads/autores/40ab6805-695e-4bd4-8d92-6d118720a486.webp', 1, 4),
(7, 'George', 'Orwell', 'Escritor y periodista británico (1903-1950), autor de 1984 y Rebelión en la granja.', '/uploads/autores/ca9a5f07-73e9-4799-8341-e78d71bd456d.png', 1, 4),
(8, 'Ray', 'Bradbury', 'Escritor estadounidense (1920-2012) de ciencia ficción y fantasía.', '/uploads/autores/1dbf0985-9096-4cf1-a8aa-8c13a966f6b5.jpg', 1, 3),
(9, 'Frank', 'Herbert', 'Escritor estadounidense (1920-1986), autor de la saga de ciencia ficción Dune.', NULL, 1, 3),
(10, 'Harper', 'Lee', 'Escritora estadounidense (1926-2016), autora de Matar a un ruiseñor.', NULL, 1, 3),
(11, 'Yuval Noah', 'Harari', 'Historiador y escritor israelí nacido en 1976, autor de Sapiens.', NULL, 1, 6),
(12, 'Mario', 'Vargas Llosa', 'Escritor peruano (1936-2025), Premio Nobel de Literatura 2010.', NULL, 1, 7),
(13, 'Umberto', 'Eco', 'Escritor y semiólogo italiano (1932-2016), autor de El nombre de la rosa.', '/uploads/autores/42b457c2-6f80-4b18-a8e7-905c467175df.jpg', 1, 8),
(14, 'Ernesto', 'Sabato', 'Escritor argentino (1911-2011), autor de El túnel y Sobre héroes y tumbas.', NULL, 1, 1),
(15, 'Adolfo', 'Bioy Casares', 'Escritor argentino (1914-1999), autor de La invención de Morel y amigo cercano de Borges.', NULL, 1, 1),
(16, 'Agatha', 'Christie', 'Escritora británica (1890-1976), una de las autoras de novela policial más leídas de la historia.', '/uploads/autores/6e00ed58-9baa-4842-9de6-880e9e201288.jpg', 1, 4),
(17, 'Héctor', 'Abad Faciolince', 'Escritor colombiano nacido en 1958, autor de El olvido que seremos.', '/uploads/autores/c8412bf1-0fb2-43b5-96dd-d88f16a1b206.png', 1, 2),
(18, 'Laura', 'Restrepo', 'Escritora colombiana nacida en 1950, autora de Delirio.', NULL, 1, 2),
(19, 'Charles', 'Dickens', 'Escritor inglés (1812-1870), autor de Oliver Twist y Grandes esperanzas.', NULL, 1, 4),
(20, 'Jane', 'Austen', 'Escritora inglesa (1775-1817), autora de Orgullo y prejuicio.', '/uploads/autores/16cd57b2-f21c-479d-857f-c4ed6326b07d.png', 1, 4),
(21, 'Gabriela', 'Mistral', 'Poeta chilena (1889-1957), autora de Desolación. Premio Nobel de Literatura 1945.', NULL, 1, 5),
(22, 'Pablo', 'Neruda', 'Poeta chileno (1904-1973), autor de Veinte poemas de amor y una canción desesperada. Premio Nobel de Literatura 1971.', '/uploads/autores/2d0ff703-5589-4db5-a160-a0f0924ebfa6.png', 1, 5),
(23, 'David', 'Grossman', 'Escritor israelí nacido en 1954, autor de Un caballo entra en un bar.', NULL, 1, 6),
(24, 'Amos', 'Oz', 'Escritor israelí (1939-2018), autor de Una historia de amor y oscuridad.', NULL, 1, 6),
(25, 'César', 'Vallejo', 'Poeta peruano (1892-1938), autor de Los heraldos negros y Trilce.', '/uploads/autores/38f87b1f-ba36-4099-ae0f-fb729ee5464f.png', 1, 7),
(26, 'Julio Ramón', 'Ribeyro', 'Escritor peruano (1929-1994), maestro del cuento, autor de La palabra del mudo.', NULL, 1, 7),
(27, 'Primo', 'Levi', 'Escritor y químico italiano (1919-1987), autor de Si esto es un hombre.', NULL, 1, 8),
(28, 'Italo', 'Calvino', 'Escritor italiano (1923-1985), autor de Las ciudades invisibles y El barón rampante.', NULL, 1, 8),
(29, 'Carlos', 'Ruiz Zafón', 'Escritor español (1964-2020), autor de La sombra del viento.', NULL, 1, 9),
(30, 'Federico', 'García Lorca', 'Poeta y dramaturgo español (1898-1936), autor de Romancero gitano y La casa de Bernarda Alba.', '/uploads/autores/4de6e74d-c138-4758-a72f-f8fd9751bb71.png', 1, 9),
(31, 'Miguel', 'de Cervantes', 'Escritor español (1547-1616), autor de Don Quijote de la Mancha.', NULL, 1, 9),
(32, 'Laura', 'Esquivel', 'Escritora mexicana nacida en 1950, autora de Como agua para chocolate.', NULL, 1, 10),
(33, 'Juan', 'Rulfo', 'Escritor mexicano (1917-1986), autor de Pedro Páramo y El llano en llamas.', NULL, 1, 10),
(34, 'Octavio', 'Paz', 'Poeta y ensayista mexicano (1914-1998), autor de El laberinto de la soledad. Premio Nobel de Literatura 1990.', NULL, 1, 10),
(35, 'Antoine', 'de Saint-Exupéry', 'Escritor y aviador francés (1900-1944), autor de El principito.', NULL, 1, 11),
(36, 'Albert', 'Camus', 'Escritor francés (1913-1960), autor de El extranjero y La peste. Premio Nobel de Literatura 1957.', '/uploads/autores/a356814c-d77f-4d0b-b3d9-d8a93d368755.png', 1, 11),
(37, 'Victor', 'Hugo', 'Escritor francés (1802-1885), autor de Los miserables y Nuestra Señora de París.', '/uploads/autores/b9739d2b-7dc3-4133-bde6-c5299bcc9962.png', 1, 11),
(38, 'Juan Carlos', 'Onetti', 'Escritor uruguayo (1909-1994), autor de El astillero.', NULL, 1, 12),
(39, 'Mario', 'Benedetti', 'Escritor y poeta uruguayo (1920-2009), autor de La tregua.', '/uploads/autores/ad2be25c-333d-4685-985b-f87c4e28a8bd.png', 1, 12),
(40, 'Eduardo', 'Galeano', 'Escritor y periodista uruguayo (1940-2015), autor de Las venas abiertas de América Latina.', '/uploads/autores/67b5eab0-ec8b-4fbf-96b5-d256fdae7a8b.png', 1, 12),
(41, 'Michael', 'Ende', 'Escritor alemán (1929-1995), autor de La historia interminable y Momo.', '/uploads/autores/1f18940d-c16b-4f0e-b817-aab669bec422.png', 1, 13),
(42, 'Thomas', 'Mann', 'Escritor alemán (1875-1955), autor de La montaña mágica. Premio Nobel de Literatura 1929.', NULL, 1, 13),
(43, 'Hermann', 'Hesse', 'Escritor alemán (1877-1962), autor de Siddhartha y El lobo estepario. Premio Nobel de Literatura 1946.', '/uploads/autores/cc85dd02-3dfd-4ad3-8c62-d25179117bb2.png', 1, 13);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `editorial`
--

CREATE TABLE `editorial` (
  `id` int(11) NOT NULL,
  `nombre` varchar(100) NOT NULL,
  `activo` tinyint(1) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `editorial`
--

INSERT INTO `editorial` (`id`, `nombre`, `activo`) VALUES
(1, 'Sudamericana', 1),
(2, 'Alfaguara', 1),
(3, 'Planeta', 1),
(4, 'Penguin Random House', 1),
(5, 'Debolsillo', 1),
(6, 'Emecé', 1),
(7, 'Minotauro', 1),
(8, 'Scribner', 1),
(9, 'Simon & Schuster', 1),
(10, 'Ace Books', 1),
(11, 'Harper Perennial', 1),
(12, 'Signet Classics', 1),
(13, 'Houghton Mifflin', 1),
(14, 'Harper', 1),
(15, 'Harper', 0),
(16, 'Penguin Books, Limited', 1),
(17, 'Booket', 1);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `ejemplar`
--

CREATE TABLE `ejemplar` (
  `id` int(11) NOT NULL,
  `id_libro` int(11) NOT NULL,
  `codigo_inventario` int(11) NOT NULL,
  `estado` enum('Disponible','Prestado') NOT NULL DEFAULT 'Disponible',
  `ubicacion` varchar(100) DEFAULT NULL,
  `activo` tinyint(4) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `ejemplar`
--

INSERT INTO `ejemplar` (`id`, `id_libro`, `codigo_inventario`, `estado`, `ubicacion`, `activo`) VALUES
(1, 1, 1001, 'Disponible', 'Estante A-1', 1),
(2, 1, 1002, 'Disponible', 'Estante A-1', 1),
(3, 2, 1003, 'Disponible', 'Estante A-1', 1),
(4, 3, 1004, 'Disponible', 'Estante A-2', 1),
(5, 3, 1005, 'Disponible', 'Estante A-2', 1),
(6, 4, 1006, 'Disponible', 'Estante A-2', 1),
(7, 5, 1007, 'Disponible', 'Estante B-1', 1),
(8, 5, 1008, 'Disponible', 'Estante B-1', 1),
(9, 5, 1009, 'Disponible', 'Estante B-1', 1),
(10, 6, 1010, 'Disponible', 'Estante B-1', 1),
(11, 6, 1011, 'Disponible', 'Estante B-1', 1),
(12, 7, 1012, 'Disponible', 'Estante B-2', 1),
(13, 8, 1013, 'Prestado', 'Estante D-1', 1),
(14, 8, 1014, 'Disponible', 'Estante D-1', 1),
(15, 9, 1015, 'Disponible', 'Estante C-1', 1),
(16, 9, 1016, 'Disponible', 'Estante C-1', 1),
(17, 10, 1017, 'Disponible', 'Estante C-1', 1),
(18, 11, 1018, 'Disponible', 'Estante C-2', 1),
(19, 11, 1019, 'Disponible', 'Estante C-2', 1),
(20, 12, 1020, 'Prestado', 'Estante C-3', 1),
(21, 12, 1021, 'Disponible', 'Estante C-3', 1),
(22, 12, 1022, 'Disponible', 'Estante C-3', 1),
(23, 13, 1023, 'Prestado', 'Estante C-3', 1),
(24, 14, 1024, 'Disponible', 'Estante D-2', 1),
(25, 14, 1025, 'Disponible', 'Estante D-2', 1),
(26, 15, 1026, 'Prestado', 'Estante D-2', 1),
(27, 16, 1027, 'Disponible', 'Estante E-1', 1),
(28, 16, 1028, 'Disponible', 'Estante E-1', 1),
(29, 17, 1029, 'Prestado', 'Estante B-3', 1),
(30, 18, 1030, 'Disponible', 'Estante B-3', 1),
(31, 19, 1031, 'Prestado', 'Estante F-1', 1),
(32, 19, 1032, 'Prestado', 'Estante F-1', 1),
(33, 20, 1033, 'Prestado', 'Estante A-3', 1),
(34, 21, 1034, 'Disponible', 'Estante A-3', 1),
(35, 22, 1035, 'Disponible', 'Estante A-4', 1),
(36, 23, 1036, 'Disponible', 'Estante F-2', 1),
(37, 23, 1037, 'Disponible', 'Estante F-2', 1),
(38, 24, 1038, 'Disponible', 'Estante F-2', 1),
(39, 25, 1039, 'Disponible', 'Estante D-1', 1),
(40, 25, 1040, 'Disponible', NULL, 1),
(41, 3, 1041, 'Disponible', 'Estante A-2', 0),
(43, 25, 1042, 'Disponible', 'Estande D-1', 0),
(44, 28, 1043, 'Disponible', 'Estante E-2', 1),
(45, 28, 1044, 'Disponible', 'Estante E-2', 1),
(46, 28, 1045, 'Disponible', 'Estante E-2', 1),
(47, 29, 1046, 'Disponible', 'Estante F-3', 1),
(48, 30, 1047, 'Disponible', 'Estante A-1', 1),
(49, 30, 1048, 'Disponible', 'Estante A-1', 1),
(50, 30, 1049, 'Disponible', 'Estante A-1', 1),
(51, 31, 1050, 'Disponible', 'Estante B-2', 1),
(52, 31, 1051, 'Disponible', 'Estante B-2', 1),
(53, 32, 1052, 'Disponible', 'Estante C-3', 1),
(54, 32, 1053, 'Disponible', 'Estante C-3', 1),
(55, 33, 1054, 'Disponible', 'Estante D-1', 1),
(56, 33, 1055, 'Disponible', 'Estante D-1', 1),
(57, 33, 1056, 'Disponible', 'Estante D-1', 1),
(58, 34, 1057, 'Disponible', 'Estante E-2', 1),
(59, 35, 1058, 'Disponible', 'Estante F-3', 1),
(60, 36, 1059, 'Disponible', 'Estante A-1', 1),
(61, 37, 1060, 'Disponible', 'Estante B-2', 1),
(62, 38, 1061, 'Disponible', 'Estante C-3', 1),
(63, 39, 1062, 'Disponible', 'Estante D-1', 1),
(64, 40, 1063, 'Disponible', 'Estante E-2', 1),
(65, 41, 1064, 'Disponible', 'Estante F-3', 1),
(66, 41, 1065, 'Disponible', 'Estante F-3', 1),
(67, 42, 1066, 'Disponible', 'Estante A-1', 1),
(68, 42, 1067, 'Disponible', 'Estante A-1', 1),
(69, 42, 1068, 'Disponible', 'Estante A-1', 1),
(70, 43, 1069, 'Disponible', 'Estante B-2', 1),
(71, 43, 1070, 'Disponible', 'Estante B-2', 1),
(72, 44, 1071, 'Disponible', 'Estante C-3', 1),
(73, 44, 1072, 'Disponible', 'Estante C-3', 1),
(74, 45, 1073, 'Disponible', 'Estante D-1', 1),
(75, 46, 1074, 'Disponible', 'Estante E-2', 1),
(76, 46, 1075, 'Disponible', 'Estante E-2', 1),
(77, 46, 1076, 'Disponible', 'Estante E-2', 1),
(78, 47, 1077, 'Disponible', 'Estante F-3', 1),
(79, 47, 1078, 'Disponible', 'Estante F-3', 1),
(80, 48, 1079, 'Disponible', 'Estante A-1', 1),
(81, 49, 1080, 'Disponible', 'Estante B-2', 1),
(82, 50, 1081, 'Disponible', 'Estante C-3', 1),
(83, 50, 1082, 'Disponible', 'Estante C-3', 1),
(84, 50, 1083, 'Disponible', 'Estante C-3', 1),
(85, 51, 1084, 'Disponible', 'Estante D-1', 1),
(86, 52, 1085, 'Disponible', 'Estante E-2', 1),
(87, 52, 1086, 'Disponible', 'Estante E-2', 1),
(88, 53, 1087, 'Disponible', 'Estante F-3', 1),
(89, 54, 1088, 'Disponible', 'Estante A-1', 1),
(90, 54, 1089, 'Disponible', 'Estante A-1', 1),
(91, 55, 1090, 'Disponible', 'Estante B-2', 1),
(92, 56, 1091, 'Disponible', 'Estante C-3', 1),
(93, 57, 1092, 'Disponible', 'Estante D-1', 1),
(94, 57, 1093, 'Disponible', 'Estante D-1', 1),
(95, 58, 1094, 'Disponible', 'Estante E-2', 1),
(96, 58, 1095, 'Disponible', 'Estante E-2', 1),
(97, 59, 1096, 'Disponible', 'Estante F-3', 1),
(98, 60, 1097, 'Disponible', 'Estante A-1', 1),
(99, 60, 1098, 'Disponible', 'Estante A-1', 1),
(100, 61, 1099, 'Disponible', 'Estante B-2', 1),
(101, 62, 1100, 'Disponible', 'Estante C-3', 1),
(102, 62, 1101, 'Disponible', 'Estante C-3', 1),
(103, 63, 1102, 'Disponible', 'Estante D-1', 1),
(104, 64, 1103, 'Disponible', 'Estante E-2', 1),
(105, 65, 1104, 'Disponible', 'Estante F-3', 1),
(106, 66, 1105, 'Disponible', 'Estante A-1', 1),
(107, 66, 1106, 'Disponible', 'Estante A-1', 1),
(108, 67, 1107, 'Disponible', 'Estante B-2', 1),
(109, 68, 1108, 'Disponible', 'Estante C-3', 1),
(110, 68, 1109, 'Disponible', 'Estante C-3', 1),
(111, 69, 1110, 'Disponible', 'Estante D-1', 1),
(112, 70, 1111, 'Disponible', 'Estante E-2', 1),
(113, 70, 1112, 'Disponible', 'Estante E-2', 1),
(114, 71, 1113, 'Disponible', 'Estante F-3', 1),
(115, 72, 1114, 'Disponible', 'Estante A-1', 1),
(116, 72, 1115, 'Disponible', 'Estante A-1', 1),
(117, 73, 1116, 'Disponible', 'Estante B-2', 1),
(118, 73, 1117, 'Disponible', 'Estante B-2', 1),
(119, 73, 1118, 'Disponible', 'Estante B-2', 1),
(120, 74, 1119, 'Disponible', 'Estante C-3', 1),
(121, 74, 1120, 'Disponible', 'Estante C-3', 1),
(122, 75, 1121, 'Disponible', 'Estante D-1', 1),
(123, 75, 1122, 'Disponible', 'Estante D-1', 1),
(124, 75, 1123, 'Disponible', 'Estante D-1', 1),
(125, 76, 1124, 'Disponible', 'Estante E-2', 1),
(126, 76, 1125, 'Disponible', 'Estante E-2', 1),
(127, 77, 1126, 'Disponible', 'Estante F-3', 1),
(128, 77, 1127, 'Disponible', 'Estante F-3', 1),
(129, 77, 1128, 'Disponible', 'Estante F-3', 1),
(130, 78, 1129, 'Disponible', 'Estante A-1', 1);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `genero`
--

CREATE TABLE `genero` (
  `id` int(11) NOT NULL,
  `nombre` varchar(100) NOT NULL,
  `activo` tinyint(4) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `genero`
--

INSERT INTO `genero` (`id`, `nombre`, `activo`) VALUES
(1, 'Novela', 1),
(2, 'Cuento', 1),
(3, 'Ciencia ficción', 1),
(4, 'Fantasía', 1),
(5, 'Terror', 1),
(6, 'Policial', 1),
(7, 'Ensayo', 1),
(8, 'Historia', 1),
(9, 'Realismo mágico', 1),
(10, 'Distopía', 1),
(11, 'Clásico', 1),
(12, 'Misterio', 1),
(13, 'Poesía', 1);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `libro`
--

CREATE TABLE `libro` (
  `id` int(11) NOT NULL,
  `titulo` varchar(255) NOT NULL,
  `id_autor` int(11) NOT NULL,
  `isbn` varchar(20) DEFAULT NULL,
  `paginas` int(11) DEFAULT NULL,
  `anio_publicacion` int(11) DEFAULT NULL,
  `idioma` varchar(50) DEFAULT NULL,
  `descripcion` text DEFAULT NULL,
  `portada` longtext DEFAULT NULL,
  `activo` tinyint(4) NOT NULL DEFAULT 1,
  `id_editorial` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `libro`
--

INSERT INTO `libro` (`id`, `titulo`, `id_autor`, `isbn`, `paginas`, `anio_publicacion`, `idioma`, `descripcion`, `portada`, `activo`, `id_editorial`) VALUES
(1, 'Ficciones', 1, NULL, 224, 1944, 'Español', 'Colección de cuentos fantásticos y metafísicos que exploran laberintos, bibliotecas infinitas y el tiempo. Una obra central de la literatura del siglo XX.', NULL, 1, 6),
(2, 'El Aleph', 1, NULL, 192, 1949, 'Español', 'Cuentos de Borges que giran en torno al infinito, la identidad y la memoria, incluido el relato que da título al libro.', '/uploads/libros/de9f8041-2120-4f01-a0ed-46a83a8e92e2.png', 1, 6),
(3, 'Rayuela', 2, NULL, 600, 1963, 'Español', 'Novela experimental que puede leerse en distintos órdenes, siguiendo la vida de Horacio Oliveira entre París y Buenos Aires.', NULL, 1, 1),
(4, 'Bestiario', 2, NULL, 160, 1951, 'Español', 'Primer libro de cuentos publicado por Cortázar, donde lo cotidiano se vuelve inquietante.', NULL, 1, 1),
(5, 'Cien años de soledad', 3, '9780307474728', 471, 1967, 'Español', 'La saga de la familia Buendía a lo largo de siete generaciones en el pueblo imaginario de Macondo.', 'https://covers.openlibrary.org/b/isbn/9780307474728-M.jpg', 1, 4),
(6, 'El amor en los tiempos del cólera', 3, NULL, 368, 1985, 'Español', 'Historia de un amor que espera más de cincuenta años para concretarse, ambientada en una ciudad del Caribe colombiano.', NULL, 1, 5),
(7, 'La casa de los espíritus', 4, NULL, 448, 1982, 'Español', 'Saga familiar de los Trueba, con lo sobrenatural y la historia política de un país latinoamericano de fondo.', NULL, 1, 5),
(8, 'It', 5, '9781501142970', 1138, 1986, 'Inglés', 'Un grupo de amigos enfrenta, de niños y de adultos, a una criatura que acecha a la ciudad de Derry.', 'https://covers.openlibrary.org/b/isbn/9781501142970-M.jpg', 1, 8),
(9, 'Fahrenheit 451', 8, '9781451673319', 249, 1953, 'Inglés', 'En una sociedad donde los libros están prohibidos y se queman, un bombero comienza a cuestionar su trabajo.', 'https://covers.openlibrary.org/b/isbn/9781451673319-M.jpg', 1, 9),
(10, 'Crónicas marcianas', 8, NULL, 288, 1950, 'Español', 'Relatos encadenados sobre la colonización de Marte por parte de la humanidad.', NULL, 1, 7),
(11, 'Dune', 9, '9780441172719', 688, 1965, 'Inglés', 'En el desértico planeta Arrakis, la familia Atreides se ve envuelta en una lucha por el control de la especia.', 'https://covers.openlibrary.org/b/isbn/9780441172719-M.jpg', 1, 10),
(12, '1984', 7, '9780451524935', 328, 1949, 'Inglés', 'En un Estado totalitario que lo vigila todo, Winston Smith intenta conservar su libertad de pensamiento.', 'https://covers.openlibrary.org/b/isbn/9780451524935-M.jpg', 1, 12),
(13, 'Rebelión en la granja', 7, NULL, 144, 1945, 'Español', 'Fábula sobre los animales de una granja que se rebelan contra su dueño y terminan bajo una nueva tiranía.', NULL, 1, NULL),
(14, 'The Hobbit', 6, '9780547928227', 300, 1937, 'Inglés', 'El hobbit Bilbo Bolsón se une a un grupo de enanos para recuperar un tesoro custodiado por un dragón.', 'https://covers.openlibrary.org/b/isbn/9780547928227-M.jpg', 1, 13),
(15, 'To Kill a Mockingbird', 10, '9780061120084', 336, 1960, 'Inglés', 'Una niña narra cómo su padre, abogado, defiende a un hombre injustamente acusado en el sur de Estados Unidos.', 'https://covers.openlibrary.org/b/isbn/9780061120084-M.jpg', 1, 11),
(16, 'Sapiens: A Brief History of Humankind', 11, '9780062316097', 464, 2011, 'Inglés', 'Recorrido por la historia de la humanidad, desde la aparición de Homo sapiens hasta la actualidad.', 'https://covers.openlibrary.org/b/isbn/9780062316097-M.jpg', 1, 14),
(17, 'La ciudad y los perros', 12, NULL, 400, 1963, 'Español', 'Novela ambientada en un colegio militar de Lima, sobre la violencia y la disciplina entre cadetes.', '/uploads/libros/12a0b8c9-c0ba-40c2-8ce7-7b1e95539b66.png', 1, 2),
(18, 'La fiesta del Chivo', 12, NULL, 520, 2000, 'Español', 'Novela sobre los últimos días de la dictadura de Trujillo en República Dominicana y sus consecuencias.', '/uploads/libros/f12917c0-f582-4c01-8f7f-b4987d98bce0.png', 1, 2),
(19, 'El nombre de la rosa', 13, NULL, 592, 1980, 'Español', 'Un monje franciscano investiga una serie de muertes misteriosas en una abadía medieval.', '/uploads/libros/72c99141-4a83-4f05-9b68-5f70cd08ef55.png', 1, 5),
(20, 'El túnel', 14, NULL, 160, 1948, 'Español', 'Un pintor narra desde la cárcel cómo su obsesión por una mujer lo llevó a cometer un crimen.', '/uploads/libros/096f8356-6bff-4745-885c-6e637c48ad39.png', 1, 3),
(21, 'Sobre héroes y tumbas', 14, NULL, 464, 1961, 'Español', 'Novela sobre el vínculo entre Martín y Alejandra en la Buenos Aires de fines de los años cincuenta.', '/uploads/libros/f588027a-655d-47f3-abaf-5462b22c8792.png', 1, 3),
(22, 'La invención de Morel', 15, NULL, 128, 1940, 'Español', 'Un fugitivo llega a una isla desierta donde aparecen personas que parecen no verlo.', '/uploads/libros/c6533710-b7f9-4278-8aef-23d67461e4ac.png', 1, 6),
(23, 'Y no quedó ninguno', 16, NULL, 288, 1939, 'Español', 'Diez personas son invitadas a una isla y mueren una a una siguiendo una antigua canción infantil.', '/uploads/libros/3c5449a1-796e-4a53-9b61-d64fef5b3e16.png', 1, NULL),
(24, 'Asesinato en el Orient Express', 16, NULL, 256, 1934, 'Español', 'El detective Hércules Poirot investiga un asesinato ocurrido en un tren detenido por la nieve.', '/uploads/libros/47fe8c68-699e-416a-a012-f9b9a74c74e9.png', 1, 3),
(25, 'El resplandor', 5, NULL, 592, 1977, 'Español', 'Un escritor se muda con su familia a un hotel aislado durante el invierno, donde fuerzas siniestras lo van dominando.', '/uploads/libros/d42089d5-0231-45a1-917b-efe0d561b5d1.png', 1, 5),
(28, 'La tregua', 39, NULL, 200, 1960, 'Español', 'Un viudo a punto de jubilarse encuentra un amor inesperado en Montevideo.', NULL, 1, 1),
(29, 'Primavera con una esquina rota', 39, NULL, 220, 1982, 'Español', 'Una familia uruguaya vive separada por el exilio y la prisión política.', NULL, 1, 1),
(30, 'La historia interminable', 41, NULL, 496, 1979, 'Español', 'Bastian encuentra un libro que lo lleva al reino de Fantasía, al borde de la desaparición.', NULL, 1, 2),
(31, 'Momo', 41, NULL, 256, 1973, 'Español', 'Una niña se enfrenta a los hombres grises, que les roban el tiempo a las personas.', NULL, 1, 2),
(32, 'Delirio', 18, NULL, 340, 2004, 'Español', 'Un hombre intenta descubrir qué llevó a su esposa a perder la razón en la Bogotá de los años ochenta.', NULL, 1, 2),
(33, 'Don Quijote de la Mancha', 31, NULL, 1250, 1605, 'Español', 'Un hidalgo enloquecido por los libros de caballerías sale a recorrer el mundo junto a Sancho Panza.', NULL, 1, 2),
(34, 'Novelas ejemplares', 31, NULL, 500, 1613, 'Español', 'Doce relatos breves sobre la sociedad española del Siglo de Oro.', NULL, 1, 2),
(35, 'La palabra del mudo', 26, NULL, 800, 1973, 'Español', 'Reunión de los cuentos del autor, protagonizados por quienes no suelen tener voz.', NULL, 1, 2),
(36, 'Crónica de San Gabriel', 26, NULL, 220, 1960, 'Español', 'Un adolescente de Lima pasa una temporada en una hacienda de la sierra peruana.', NULL, 1, 2),
(37, 'El astillero', 38, NULL, 240, 1961, 'Español', 'Larsen intenta dirigir un astillero en ruinas en la ciudad imaginaria de Santa María.', NULL, 1, 2),
(38, 'La vida breve', 38, NULL, 500, 1950, 'Español', 'Un publicista de Buenos Aires inventa otra vida y crea la ciudad de Santa María.', NULL, 1, 2),
(39, 'Desolación', 21, NULL, 200, 1922, 'Español', 'Primer libro de la autora, marcado por el dolor, la fe y el amor perdido.', NULL, 1, 3),
(40, 'Tala', 21, NULL, 220, 1938, 'Español', 'Poemario de madurez que reflexiona sobre la muerte, la tierra y América.', NULL, 1, 3),
(41, 'El olvido que seremos', 17, NULL, 280, 2006, 'Español', 'Retrato del padre del autor, médico y defensor de los derechos humanos asesinado en Medellín.', NULL, 1, 3),
(42, 'La sombra del viento', 29, NULL, 576, 2001, 'Español', 'En la Barcelona de posguerra, un niño descubre un libro cuyo autor alguien intenta borrar.', NULL, 1, 3),
(43, 'El juego del ángel', 29, NULL, 672, 2008, 'Español', 'Un escritor recibe un extraño encargo de un misterioso editor en la Barcelona de los años veinte.', NULL, 1, 3),
(44, 'Si esto es un hombre', 27, NULL, 208, 1947, 'Español', 'El testimonio del autor sobre su paso por el campo de exterminio de Auschwitz.', NULL, 1, 3),
(45, 'La tregua', 27, NULL, 280, 1963, 'Español', 'El largo regreso del autor a Italia después de la liberación de Auschwitz.', NULL, 1, 3),
(46, 'Pedro Páramo', 33, NULL, 128, 1955, 'Español', 'Juan Preciado llega a Comala a buscar a su padre y encuentra un pueblo habitado por muertos.', NULL, 1, 3),
(47, 'El llano en llamas', 33, NULL, 192, 1953, 'Español', 'Cuentos sobre la vida dura del campo mexicano después de la Revolución.', '/uploads/libros/65fd3dba-a2c9-4545-a141-433d33d3cd38.png', 1, 3),
(48, 'Los heraldos negros', 25, NULL, 120, 1919, 'Español', 'Primer poemario del autor, sobre el dolor humano y el desamparo.', '/uploads/libros/75bb1d2f-fdb6-4ba5-a0ae-702045308c3f.png', 1, 3),
(49, 'Trilce', 25, NULL, 150, 1922, 'Español', 'Obra de vanguardia que renovó el lenguaje de la poesía en español.', '/uploads/libros/2a338808-6f88-43b8-bb0a-d7c8e60f7772.png', 1, 3),
(50, 'Orgullo y prejuicio', 20, NULL, 432, 1813, 'Español', 'Elizabeth Bennet y el señor Darcy superan sus primeras impresiones en la Inglaterra rural.', '/uploads/libros/830ee175-b9f8-484d-bc92-c519773fa48a.png', 1, 4),
(51, 'Sentido y sensibilidad', 20, NULL, 400, 1811, 'Español', 'Dos hermanas de carácter opuesto enfrentan el amor y las dificultades económicas.', '/uploads/libros/a32d1b77-359d-4a3a-9da5-22915a581d35.png', 1, 4),
(52, 'Oliver Twist', 19, NULL, 544, 1838, 'Español', 'Un huérfano escapa de un hospicio y cae en una banda de ladrones en Londres.', '/uploads/libros/06f6462f-9c95-40da-934a-9a3608a19864.png', 1, 4),
(53, 'Grandes esperanzas', 19, NULL, 544, 1861, 'Español', 'El joven Pip recibe una fortuna de origen desconocido que cambia su vida.', '/uploads/libros/0e0768f1-a0c1-4402-8b55-2e5a67b44248.png', 1, 4),
(54, 'Siddhartha', 43, NULL, 192, 1922, 'Español', 'Un joven de la India antigua abandona su vida acomodada en busca de la iluminación.', '/uploads/libros/0510fa31-20eb-42e7-835c-7db559f0da79.png', 1, 5),
(55, 'Demian', 43, NULL, 200, 1919, 'Español', 'Emil Sinclair crece entre el mundo ordenado de su familia y otro más oscuro, guiado por su amigo Demian.', '/uploads/libros/a53f1f40-6374-4b1c-8ccd-5e637d77ad89.png', 1, 5),
(56, 'La montaña mágica', 42, NULL, 880, 1924, 'Español', 'Una visita breve a un sanatorio de los Alpes se convierte en una estadía de siete años.', '/uploads/libros/39e23dd8-2f1d-4c94-9842-bc02e9d59983.png', 1, 5),
(57, 'Muerte en Venecia', 42, NULL, 140, 1912, 'Español', 'Un escritor maduro viaja a Venecia y queda obsesionado con la belleza de un joven.', '/uploads/libros/c2cc9b3e-e003-4b3e-b3d0-e38925e5a0ea.png', 1, 5),
(58, 'Veinte poemas de amor y una canción desesperada', 22, NULL, 100, 1924, 'Español', 'Poemario de juventud sobre el amor y la pérdida, uno de los más leídos en lengua española.', '/uploads/libros/f30f5cee-130c-4bdd-bf51-eb5e32564630.png', 1, 17),
(59, 'Canto general', 22, NULL, 600, 1950, 'Español', 'Extensa obra poética sobre la historia, la naturaleza y las luchas de América Latina.', '/uploads/libros/bbce06f4-0ab0-4753-af43-cfc2b9b0d835.png', 1, 5),
(60, 'Romancero gitano', 30, NULL, 150, 1928, 'Español', 'Romances que unen la tradición popular con imágenes modernas en una Andalucía gitana.', '/uploads/libros/649ef662-888c-4953-8c7f-c89da35d8d76.png', 1, 5),
(61, 'Poeta en Nueva York', 30, NULL, 220, 1940, 'Español', 'Poemas escritos durante la estadía del autor en Nueva York, publicados después de su muerte.', '/uploads/libros/ac6ff608-0f2b-41e4-a0af-8dd1c31405ce.png', 1, 5),
(62, 'Los miserables', 37, NULL, 1500, 1862, 'Español', 'Jean Valjean busca redimirse mientras un inspector lo persigue en la Francia del siglo XIX.', '/uploads/libros/121e52b1-c930-431f-9934-7828a5d466a8.png', 1, 5),
(63, 'Nuestra Señora de París', 37, NULL, 650, 1831, 'Español', 'El campanero Quasimodo y la gitana Esmeralda, en el París medieval.', '/uploads/libros/595ee012-7f6d-4a38-be9e-3c52c4257734.png', 1, 5),
(64, 'Una historia de amor y oscuridad', 24, NULL, 750, 2002, 'Español', 'Las memorias del autor, entre su infancia en Jerusalén y la historia de su familia.', '/uploads/libros/6ebd5797-c3af-401b-8f9d-dc7a5bde0308.png', 1, 5),
(65, 'Judas', 24, NULL, 340, 2014, 'Español', 'En la Jerusalén de 1959, un estudiante se muda a una casa llena de secretos y discusiones sobre la traición.', '/uploads/libros/6f256a04-9cd5-4017-a77a-ef727311e92a.png', 1, 5),
(66, 'Un caballo entra en un bar', 23, NULL, 224, 2014, 'Español', 'Un comediante convierte su última función en un doloroso ajuste de cuentas con su pasado.', '/uploads/libros/b0255fda-5207-43d2-b9bc-c38c5a85d3c5.png', 1, 5),
(67, 'Hasta el fin de la tierra', 23, NULL, 750, 2008, 'Español', 'Una madre israelí sale a caminar por Galilea para no recibir la noticia que teme sobre su hijo soldado.', NULL, 0, 5),
(68, 'Las ciudades invisibles', 28, NULL, 176, 1972, 'Español', 'Marco Polo le describe al Kublai Kan las ciudades imaginarias de su imperio.', '/uploads/libros/b6542497-63af-4eff-a921-df3519483d41.png', 1, 5),
(69, 'El barón rampante', 28, NULL, 288, 1957, 'Español', 'Un joven noble sube a un árbol a los doce años y decide no volver a bajar.', '/uploads/libros/1f0afcee-7eca-405d-91ec-18baede5da3f.png', 1, 5),
(70, 'El laberinto de la soledad', 34, NULL, 350, 1950, 'Español', 'Ensayo sobre la identidad, la historia y las costumbres del pueblo mexicano.', '/uploads/libros/240b8ee2-8e43-4d97-b14b-ebe150e62117.png', 1, 5),
(71, 'Piedra de sol', 34, NULL, 60, 1957, 'Español', 'Extenso poema circular inspirado en el calendario azteca.', NULL, 0, 5),
(72, 'Como agua para chocolate', 32, NULL, 272, 1989, 'Español', 'Tita expresa a través de la cocina un amor prohibido, en el México de la Revolución.', '/uploads/libros/1b09853b-6607-4929-9c55-eb01cdb60213.png', 1, 5),
(73, 'Las venas abiertas de América Latina', 40, NULL, 400, 1971, 'Español', 'Recorrido por la historia de la explotación económica y política de América Latina.', '/uploads/libros/28c803f0-2fce-4e2a-94f0-155e9782a2e8.png', 1, 5),
(74, 'El libro de los abrazos', 40, NULL, 270, 1989, 'Español', 'Relatos muy breves sobre la memoria, el amor y la vida cotidiana.', '/uploads/libros/b59581a8-47ed-47d4-9cdf-3d4ee521cc78.png', 1, 5),
(75, 'El extranjero', 36, NULL, 128, 1942, 'Español', 'Meursault mata a un hombre en una playa de Argelia y enfrenta un juicio por su indiferencia.', '/uploads/libros/3aa45f13-dece-470f-a795-7cd6932112b7.png', 1, 6),
(76, 'La peste', 36, NULL, 320, 1947, 'Español', 'Una epidemia aísla a la ciudad de Orán y pone a prueba a sus habitantes.', '/uploads/libros/3b2afb9a-72ad-4ec1-a568-88736a228cbf.png', 1, 6),
(77, 'El principito', 35, NULL, 96, 1943, 'Español', 'Un aviador perdido en el desierto conoce a un pequeño príncipe llegado de otro planeta.', '/uploads/libros/ecfc58ef-a05e-4afd-96fe-a16f0bcb5caa.png', 1, 6),
(78, 'Vuelo nocturno', 35, NULL, 150, 1931, 'Español', 'Los pilotos del correo aéreo arriesgan la vida volando de noche sobre Sudamérica.', '/uploads/libros/e8494d7e-3dce-47d2-8e45-59a8fc157847.png', 1, 5);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `libro_genero`
--

CREATE TABLE `libro_genero` (
  `id_libro` int(11) NOT NULL,
  `id_genero` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `libro_genero`
--

INSERT INTO `libro_genero` (`id_libro`, `id_genero`) VALUES
(1, 2),
(1, 11),
(2, 2),
(2, 11),
(3, 1),
(3, 11),
(4, 2),
(5, 1),
(5, 9),
(5, 11),
(6, 1),
(6, 11),
(7, 1),
(7, 9),
(8, 1),
(8, 5),
(9, 3),
(9, 10),
(9, 11),
(10, 2),
(10, 3),
(11, 1),
(11, 3),
(12, 3),
(12, 10),
(12, 11),
(13, 1),
(13, 10),
(13, 11),
(14, 1),
(14, 4),
(14, 11),
(15, 1),
(15, 11),
(16, 7),
(16, 8),
(17, 1),
(18, 1),
(19, 1),
(19, 12),
(20, 1),
(20, 11),
(21, 1),
(22, 1),
(22, 3),
(22, 11),
(23, 6),
(23, 12),
(24, 6),
(24, 12),
(25, 1),
(25, 5),
(28, 1),
(28, 11),
(29, 1),
(30, 1),
(30, 4),
(31, 1),
(31, 4),
(32, 1),
(33, 1),
(33, 11),
(34, 2),
(34, 11),
(35, 2),
(35, 11),
(36, 1),
(37, 1),
(37, 11),
(38, 1),
(38, 11),
(39, 11),
(39, 13),
(40, 11),
(40, 13),
(41, 1),
(41, 8),
(42, 1),
(42, 12),
(43, 1),
(43, 12),
(44, 8),
(44, 11),
(45, 8),
(46, 1),
(46, 9),
(46, 11),
(47, 2),
(47, 11),
(48, 11),
(48, 13),
(49, 11),
(49, 13),
(50, 1),
(50, 11),
(51, 1),
(51, 11),
(52, 1),
(52, 11),
(53, 1),
(53, 11),
(54, 1),
(54, 11),
(55, 1),
(55, 11),
(56, 1),
(56, 11),
(57, 1),
(57, 11),
(58, 11),
(58, 13),
(59, 8),
(59, 13),
(60, 11),
(60, 13),
(61, 11),
(61, 13),
(62, 1),
(62, 11),
(63, 1),
(63, 11),
(64, 1),
(64, 8),
(65, 1),
(66, 1),
(67, 1),
(68, 1),
(68, 4),
(69, 1),
(69, 4),
(70, 7),
(70, 11),
(71, 11),
(71, 13),
(72, 1),
(72, 9),
(73, 7),
(73, 8),
(74, 2),
(75, 1),
(75, 11),
(76, 1),
(76, 11),
(77, 4),
(77, 11),
(78, 1);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `nacionalidad`
--

CREATE TABLE `nacionalidad` (
  `id` int(11) NOT NULL,
  `nombre` varchar(50) NOT NULL,
  `activo` tinyint(4) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `nacionalidad`
--

INSERT INTO `nacionalidad` (`id`, `nombre`, `activo`) VALUES
(0, 'Desconocida', 1),
(1, 'Argentina', 1),
(2, 'Colombia', 1),
(3, 'Estados Unidos', 1),
(4, 'Inglaterra', 1),
(5, 'Chile', 1),
(6, 'Israel', 1),
(7, 'Perú', 1),
(8, 'Italia', 1),
(9, 'España', 1),
(10, 'Mexico', 1),
(11, 'Francia', 1),
(12, 'Uruguay', 1),
(13, 'Alemania', 1);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `pago`
--

CREATE TABLE `pago` (
  `id` int(11) NOT NULL,
  `id_afiliado` int(11) NOT NULL,
  `creado_por_user_id` int(11) NOT NULL,
  `creado_en` datetime DEFAULT NULL,
  `anulado_por_user_id` int(11) DEFAULT NULL,
  `anulado_en` datetime DEFAULT NULL,
  `fecha_pago` date NOT NULL,
  `monto` decimal(10,2) NOT NULL,
  `concepto` varchar(255) NOT NULL,
  `saldado` tinyint(4) NOT NULL DEFAULT 1,
  `activo` tinyint(4) NOT NULL DEFAULT 1,
  `tipo` enum('Cuota','Multa') NOT NULL DEFAULT 'Cuota',
  `id_suscripcion` int(11) DEFAULT NULL,
  `id_prestamo` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `pago`
--

INSERT INTO `pago` (`id`, `id_afiliado`, `creado_por_user_id`, `creado_en`, `anulado_por_user_id`, `anulado_en`, `fecha_pago`, `monto`, `concepto`, `saldado`, `activo`, `tipo`, `id_suscripcion`, `id_prestamo`) VALUES
(1, 1, 2, '2026-08-24 09:37:00', NULL, NULL, '2026-08-24', 8000.00, 'Cuota 24/08/2026 al 22/09/2026', 1, 1, 'Cuota', 1, NULL),
(2, 1, 3, '2026-09-23 10:14:00', NULL, NULL, '2026-09-23', 9000.00, 'Cuota 23/09/2026 al 23/10/2026', 1, 1, 'Cuota', 2, NULL),
(3, 1, 2, '2026-10-24 10:51:00', NULL, NULL, '2026-10-24', 10000.00, 'Cuota 24/10/2026 al 23/11/2026', 1, 1, 'Cuota', 3, NULL),
(4, 2, 3, '2026-09-23 11:28:00', NULL, NULL, '2026-09-23', 9000.00, 'Cuota 23/09/2026 al 23/10/2026', 1, 1, 'Cuota', 4, NULL),
(5, 2, 2, '2026-10-24 12:05:00', NULL, NULL, '2026-10-24', 10000.00, 'Cuota 24/10/2026 al 23/11/2026', 1, 1, 'Cuota', 5, NULL),
(6, 3, 3, '2026-10-24 12:42:00', NULL, NULL, '2026-10-24', 10000.00, 'Cuota 24/10/2026 al 23/11/2026', 1, 1, 'Cuota', 6, NULL),
(7, 4, 2, '2026-09-23 09:19:00', NULL, NULL, '2026-09-23', 9000.00, 'Cuota 23/09/2026 al 23/10/2026', 1, 1, 'Cuota', 7, NULL),
(8, 4, 3, '2026-10-24 09:56:00', NULL, NULL, '2026-10-24', 10000.00, 'Cuota 24/10/2026 al 23/11/2026', 1, 1, 'Cuota', 8, NULL),
(9, 5, 2, '2026-10-24 10:33:00', NULL, NULL, '2026-10-24', 10000.00, 'Cuota 24/10/2026 al 23/11/2026', 1, 1, 'Cuota', 9, NULL),
(10, 6, 3, '2026-10-24 11:10:00', NULL, NULL, '2026-10-24', 10000.00, 'Cuota 24/10/2026 al 23/11/2026', 1, 1, 'Cuota', 10, NULL),
(11, 7, 2, '2026-10-24 11:47:00', NULL, NULL, '2026-10-24', 10000.00, 'Cuota 24/10/2026 al 23/11/2026', 1, 1, 'Cuota', 11, NULL),
(12, 8, 3, '2026-10-24 12:24:00', NULL, NULL, '2026-10-24', 10000.00, 'Cuota 24/10/2026 al 23/11/2026', 1, 1, 'Cuota', 12, NULL),
(13, 10, 3, '2026-09-04 09:01:00', NULL, NULL, '2026-09-04', 8000.00, 'Cuota 04/09/2026 al 03/10/2026', 1, 1, 'Cuota', 14, NULL),
(14, 2, 3, '2026-08-24 09:38:00', NULL, NULL, '2026-08-24', 8000.00, 'Cuota 24/08/2026 al 22/09/2026', 1, 1, 'Cuota', 16, NULL),
(16, 9, 3, '2026-10-31 10:52:00', 2, '2026-10-31 16:08:00', '2026-10-31', 10000.00, 'Cuota 31/10/2026 al 30/11/2026', 1, 0, 'Cuota', 13, NULL),
(17, 1, 3, '2026-10-07 15:46:00', NULL, NULL, '2026-10-07', 400.00, 'Multa por 4 día(s) de atraso - Préstamo #2', 1, 1, 'Multa', NULL, 2),
(18, 2, 3, '2026-10-15 17:32:00', NULL, NULL, '2026-10-15', 700.00, 'Multa por 7 día(s) de atraso - Préstamo #4', 1, 1, 'Multa', NULL, 4),
(19, 8, 3, '2026-11-02 14:22:00', NULL, NULL, '2026-11-02', 300.00, 'Multa por 3 día(s) de atraso - Préstamo #14', 0, 1, 'Multa', NULL, 14),
(20, 6, 2, '2026-11-03 17:26:00', NULL, NULL, '2026-11-03', 200.00, 'Multa por 2 día(s) de atraso - Préstamo #22', 1, 1, 'Multa', NULL, 22);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `prestamo`
--

CREATE TABLE `prestamo` (
  `id` int(11) NOT NULL,
  `id_afiliado` int(11) NOT NULL,
  `id_ejemplar` int(11) NOT NULL,
  `creado_por_user_id` int(11) NOT NULL,
  `creado_en` datetime DEFAULT NULL,
  `anulado_por_user_id` int(11) DEFAULT NULL,
  `devuelto_por_user_id` int(11) DEFAULT NULL,
  `devuelto_en` datetime DEFAULT NULL,
  `anulado_en` datetime DEFAULT NULL,
  `fecha_prestamo` date NOT NULL,
  `fecha_devolucion_pactada` date NOT NULL,
  `fecha_devolucion_real` date DEFAULT NULL,
  `estado` varchar(50) NOT NULL,
  `activo` tinyint(4) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `prestamo`
--

INSERT INTO `prestamo` (`id`, `id_afiliado`, `id_ejemplar`, `creado_por_user_id`, `creado_en`, `anulado_por_user_id`, `devuelto_por_user_id`, `devuelto_en`, `anulado_en`, `fecha_prestamo`, `fecha_devolucion_pactada`, `fecha_devolucion_real`, `estado`, `activo`) VALUES
(1, 1, 1, 2, '2026-08-30 09:37:00', NULL, 4, '2026-09-12 14:53:00', NULL, '2026-08-30', '2026-09-13', '2026-09-12', 'Devuelto', 1),
(2, 1, 4, 3, '2026-09-19 10:14:00', NULL, 3, '2026-10-07 15:46:00', NULL, '2026-09-19', '2026-10-03', '2026-10-07', 'Devuelto', 1),
(3, 2, 7, 2, '2026-09-02 10:51:00', NULL, 3, '2026-09-16 16:39:00', NULL, '2026-09-02', '2026-09-16', '2026-09-16', 'Devuelto', 1),
(4, 2, 10, 3, '2026-09-24 11:28:00', NULL, 3, '2026-10-15 17:32:00', NULL, '2026-09-24', '2026-10-08', '2026-10-15', 'Devuelto', 1),
(5, 4, 15, 2, '2026-09-26 12:05:00', NULL, 5, '2026-10-09 14:25:00', NULL, '2026-09-26', '2026-10-10', '2026-10-09', 'Devuelto', 1),
(6, 1, 18, 3, '2026-10-09 12:42:00', NULL, 3, '2026-10-22 15:18:00', NULL, '2026-10-09', '2026-10-23', '2026-10-22', 'Devuelto', 1),
(7, 3, 20, 2, '2026-10-25 09:19:00', NULL, NULL, NULL, NULL, '2026-10-25', '2026-11-08', NULL, 'En progreso', 1),
(8, 5, 23, 3, '2026-10-26 09:56:00', NULL, NULL, NULL, NULL, '2026-10-26', '2026-11-09', NULL, 'En progreso', 1),
(9, 6, 26, 2, '2026-10-27 10:33:00', NULL, NULL, NULL, NULL, '2026-10-27', '2026-11-10', NULL, 'En progreso', 1),
(10, 4, 31, 3, '2026-10-28 11:10:00', NULL, NULL, NULL, NULL, '2026-10-28', '2026-11-11', NULL, 'En progreso', 1),
(11, 1, 33, 2, '2026-10-30 11:47:00', NULL, NULL, NULL, NULL, '2026-10-30', '2026-11-13', NULL, 'En progreso', 1),
(12, 2, 13, 3, '2026-10-31 12:24:00', NULL, NULL, NULL, NULL, '2026-10-31', '2026-11-14', NULL, 'En progreso', 1),
(13, 5, 2, 2, '2026-10-25 09:01:00', NULL, 4, '2026-10-29 17:29:00', NULL, '2026-10-25', '2026-11-08', '2026-10-29', 'Devuelto', 1),
(14, 8, 38, 3, '2026-10-25 09:38:00', NULL, 3, '2026-11-02 14:22:00', NULL, '2026-10-25', '2026-10-30', '2026-11-02', 'Devuelto', 1),
(15, 7, 29, 2, '2026-10-25 10:15:00', NULL, NULL, NULL, NULL, '2026-10-25', '2026-11-01', NULL, 'En progreso', 1),
(16, 3, 40, 3, '2026-11-02 10:52:00', 2, NULL, NULL, '2026-11-02 16:08:00', '2026-11-02', '2026-11-16', NULL, 'En progreso', 0),
(19, 6, 39, 2, '2026-11-03 12:43:00', 2, NULL, NULL, '2026-11-03 14:47:00', '2026-11-03', '2026-11-17', NULL, 'En progreso', 0),
(20, 6, 40, 2, '2026-11-03 09:20:00', 2, NULL, NULL, '2026-11-03 15:40:00', '2026-11-03', '2026-11-17', NULL, 'En progreso', 0),
(21, 6, 40, 2, '2026-11-03 09:57:00', NULL, 3, '2026-11-03 16:33:00', NULL, '2026-11-03', '2026-11-17', '2026-11-03', 'Devuelto', 1),
(22, 6, 14, 2, '2026-10-20 10:34:00', NULL, 2, '2026-11-03 17:26:00', NULL, '2026-10-20', '2026-11-01', '2026-11-03', 'Devuelto', 1),
(25, 3, 16, 2, '2026-11-04 12:25:00', NULL, 4, '2026-11-04 16:05:00', NULL, '2026-11-04', '2026-12-31', '2026-11-04', 'Devuelto', 1),
(26, 1, 1, 2, '2026-11-04 09:02:00', NULL, 5, '2026-11-04 16:58:00', NULL, '2026-11-04', '2026-12-31', '2026-11-04', 'Devuelto', 1),
(27, 1, 1, 2, '2026-11-04 09:39:00', NULL, 3, '2026-11-04 17:51:00', NULL, '2026-11-04', '2026-12-31', '2026-11-04', 'Devuelto', 1),
(28, 1, 49, 4, '2025-11-29 09:37:00', NULL, 5, '2025-12-07 14:53:00', NULL, '2025-11-29', '2025-12-13', '2025-12-07', 'Devuelto', 1),
(29, 1, 56, 5, '2025-11-29 10:14:00', NULL, 3, '2025-12-08 15:46:00', NULL, '2025-11-29', '2025-12-13', '2025-12-08', 'Devuelto', 1),
(30, 2, 57, 3, '2025-11-29 10:51:00', NULL, 4, '2025-12-09 16:39:00', NULL, '2025-11-29', '2025-12-13', '2025-12-09', 'Devuelto', 1),
(31, 2, 116, 4, '2025-11-29 11:28:00', NULL, 5, '2025-12-06 17:32:00', NULL, '2025-11-29', '2025-12-13', '2025-12-06', 'Devuelto', 1),
(32, 1, 123, 5, '2025-11-29 12:05:00', NULL, 3, '2025-12-07 14:25:00', NULL, '2025-11-29', '2025-12-13', '2025-12-07', 'Devuelto', 1),
(33, 1, 7, 3, '2025-12-29 12:42:00', NULL, 4, '2026-01-07 15:18:00', NULL, '2025-12-29', '2026-01-12', '2026-01-07', 'Devuelto', 1),
(34, 1, 20, 4, '2025-12-29 09:19:00', NULL, 5, '2026-01-08 16:11:00', NULL, '2025-12-29', '2026-01-12', '2026-01-08', 'Devuelto', 1),
(35, 1, 21, 5, '2025-12-29 09:56:00', NULL, 3, '2026-01-05 17:04:00', NULL, '2025-12-29', '2026-01-12', '2026-01-05', 'Devuelto', 1),
(36, 2, 50, 3, '2025-12-29 10:33:00', NULL, 4, '2026-01-06 17:57:00', NULL, '2025-12-29', '2026-01-12', '2026-01-06', 'Devuelto', 1),
(37, 2, 57, 4, '2025-12-29 11:10:00', NULL, 5, '2026-01-07 14:50:00', NULL, '2025-12-29', '2026-01-12', '2026-01-07', 'Devuelto', 1),
(38, 1, 117, 5, '2025-12-29 11:47:00', NULL, 3, '2026-01-08 15:43:00', NULL, '2025-12-29', '2026-01-12', '2026-01-08', 'Devuelto', 1),
(39, 4, 1, 3, '2026-01-28 12:24:00', NULL, 4, '2026-02-04 16:36:00', NULL, '2026-01-28', '2026-02-11', '2026-02-04', 'Devuelto', 1),
(40, 3, 7, 4, '2026-01-28 09:01:00', NULL, 5, '2026-02-05 17:29:00', NULL, '2026-01-28', '2026-02-11', '2026-02-05', 'Devuelto', 1),
(41, 3, 8, 5, '2026-01-28 09:38:00', NULL, 3, '2026-02-06 14:22:00', NULL, '2026-01-28', '2026-02-11', '2026-02-06', 'Devuelto', 1),
(42, 2, 21, 3, '2026-01-28 10:15:00', NULL, 4, '2026-02-07 15:15:00', NULL, '2026-01-28', '2026-02-11', '2026-02-07', 'Devuelto', 1),
(43, 1, 22, 4, '2026-01-28 10:52:00', NULL, 5, '2026-02-04 16:08:00', NULL, '2026-01-28', '2026-02-11', '2026-02-04', 'Devuelto', 1),
(44, 4, 44, 5, '2026-01-28 11:29:00', NULL, 3, '2026-02-05 17:01:00', NULL, '2026-01-28', '2026-02-11', '2026-02-05', 'Devuelto', 1),
(45, 4, 82, 3, '2026-01-28 12:06:00', NULL, 4, '2026-02-06 17:54:00', NULL, '2026-01-28', '2026-02-11', '2026-02-06', 'Devuelto', 1),
(46, 3, 118, 4, '2026-01-28 12:43:00', NULL, 5, '2026-02-07 14:47:00', NULL, '2026-01-28', '2026-02-11', '2026-02-07', 'Devuelto', 1),
(47, 3, 1, 5, '2026-02-27 09:20:00', NULL, 3, '2026-03-06 15:40:00', NULL, '2026-02-27', '2026-03-13', '2026-03-06', 'Devuelto', 1),
(48, 2, 2, 3, '2026-02-27 09:57:00', NULL, 4, '2026-03-07 16:33:00', NULL, '2026-02-27', '2026-03-13', '2026-03-07', 'Devuelto', 1),
(49, 1, 8, 4, '2026-02-27 10:34:00', NULL, 5, '2026-03-08 17:26:00', NULL, '2026-02-27', '2026-03-13', '2026-03-08', 'Devuelto', 1),
(50, 4, 9, 5, '2026-02-27 11:11:00', NULL, 3, '2026-03-09 14:19:00', NULL, '2026-02-27', '2026-03-13', '2026-03-09', 'Devuelto', 1),
(51, 4, 45, 3, '2026-02-27 11:48:00', NULL, 4, '2026-03-06 15:12:00', NULL, '2026-02-27', '2026-03-13', '2026-03-06', 'Devuelto', 1),
(52, 3, 52, 4, '2026-02-27 12:25:00', NULL, 5, '2026-03-07 16:05:00', NULL, '2026-02-27', '2026-03-13', '2026-03-07', 'Devuelto', 1),
(53, 3, 67, 5, '2026-02-27 09:02:00', NULL, 3, '2026-03-08 16:58:00', NULL, '2026-02-27', '2026-03-13', '2026-03-08', 'Devuelto', 1),
(54, 2, 68, 3, '2026-02-27 09:39:00', NULL, 4, '2026-03-09 17:51:00', NULL, '2026-02-27', '2026-03-13', '2026-03-09', 'Devuelto', 1),
(55, 1, 82, 4, '2026-02-27 10:16:00', NULL, 5, '2026-03-06 14:44:00', NULL, '2026-02-27', '2026-03-13', '2026-03-06', 'Devuelto', 1),
(56, 4, 83, 5, '2026-02-27 10:53:00', NULL, 3, '2026-03-07 15:37:00', NULL, '2026-02-27', '2026-03-13', '2026-03-07', 'Devuelto', 1),
(57, 4, 84, 3, '2026-02-27 11:30:00', NULL, 4, '2026-03-08 16:30:00', NULL, '2026-02-27', '2026-03-13', '2026-03-08', 'Devuelto', 1),
(58, 3, 1, 4, '2026-03-29 12:07:00', NULL, 5, '2026-04-08 17:23:00', NULL, '2026-03-29', '2026-04-12', '2026-04-08', 'Devuelto', 1),
(59, 2, 2, 5, '2026-03-29 12:44:00', NULL, 3, '2026-04-05 14:16:00', NULL, '2026-03-29', '2026-04-12', '2026-04-05', 'Devuelto', 1),
(60, 2, 9, 3, '2026-03-29 09:21:00', NULL, 4, '2026-04-06 15:09:00', NULL, '2026-03-29', '2026-04-12', '2026-04-06', 'Devuelto', 1),
(61, 1, 46, 4, '2026-03-29 09:58:00', NULL, 5, '2026-04-07 16:02:00', NULL, '2026-03-29', '2026-04-12', '2026-04-07', 'Devuelto', 1),
(62, 4, 55, 5, '2026-03-29 10:35:00', NULL, 3, '2026-04-08 16:55:00', NULL, '2026-03-29', '2026-04-12', '2026-04-08', 'Devuelto', 1),
(63, 4, 56, 3, '2026-03-29 11:12:00', NULL, 4, '2026-04-05 17:48:00', NULL, '2026-03-29', '2026-04-12', '2026-04-05', 'Devuelto', 1),
(64, 3, 69, 4, '2026-03-29 11:49:00', NULL, 5, '2026-04-06 14:41:00', NULL, '2026-03-29', '2026-04-12', '2026-04-06', 'Devuelto', 1),
(65, 2, 76, 5, '2026-03-29 12:26:00', NULL, 3, '2026-04-07 15:34:00', NULL, '2026-03-29', '2026-04-12', '2026-04-07', 'Devuelto', 1),
(66, 2, 83, 3, '2026-03-29 09:03:00', NULL, 4, '2026-04-08 16:27:00', NULL, '2026-03-29', '2026-04-12', '2026-04-08', 'Devuelto', 1),
(67, 1, 84, 4, '2026-03-29 09:40:00', NULL, 5, '2026-04-05 17:20:00', NULL, '2026-03-29', '2026-04-12', '2026-04-05', 'Devuelto', 1),
(68, 4, 128, 5, '2026-03-29 10:17:00', NULL, 3, '2026-04-06 14:13:00', NULL, '2026-03-29', '2026-04-12', '2026-04-06', 'Devuelto', 1),
(69, 4, 48, 3, '2026-04-28 10:54:00', NULL, 4, '2026-05-07 15:06:00', NULL, '2026-04-28', '2026-05-12', '2026-05-07', 'Devuelto', 1),
(70, 3, 55, 4, '2026-04-28 11:31:00', NULL, 5, '2026-05-08 15:59:00', NULL, '2026-04-28', '2026-05-12', '2026-05-08', 'Devuelto', 1),
(71, 2, 56, 5, '2026-04-28 12:08:00', NULL, 3, '2026-05-05 16:52:00', NULL, '2026-04-28', '2026-05-12', '2026-05-05', 'Devuelto', 1),
(72, 5, 57, 3, '2026-04-28 12:45:00', NULL, 4, '2026-05-06 17:45:00', NULL, '2026-04-28', '2026-05-12', '2026-05-06', 'Devuelto', 1),
(73, 1, 77, 4, '2026-04-28 09:22:00', NULL, 5, '2026-05-07 14:38:00', NULL, '2026-04-28', '2026-05-12', '2026-05-07', 'Devuelto', 1),
(74, 4, 122, 5, '2026-04-28 09:59:00', NULL, 3, '2026-05-08 15:31:00', NULL, '2026-04-28', '2026-05-12', '2026-05-08', 'Devuelto', 1),
(75, 3, 129, 3, '2026-04-28 10:36:00', NULL, 4, '2026-05-05 16:24:00', NULL, '2026-04-28', '2026-05-12', '2026-05-05', 'Devuelto', 1),
(76, 3, 49, 4, '2026-05-28 11:13:00', NULL, 5, '2026-06-05 17:17:00', NULL, '2026-05-28', '2026-06-11', '2026-06-05', 'Devuelto', 1),
(77, 2, 56, 5, '2026-05-28 11:50:00', NULL, 3, '2026-06-06 14:10:00', NULL, '2026-05-28', '2026-06-11', '2026-06-06', 'Devuelto', 1),
(78, 5, 57, 3, '2026-05-28 12:27:00', NULL, 4, '2026-06-07 15:03:00', NULL, '2026-05-28', '2026-06-11', '2026-06-07', 'Devuelto', 1),
(79, 1, 116, 4, '2026-05-28 09:04:00', NULL, 5, '2026-06-04 15:56:00', NULL, '2026-05-28', '2026-06-11', '2026-06-04', 'Devuelto', 1),
(80, 4, 123, 5, '2026-05-28 09:41:00', NULL, 3, '2026-06-05 16:49:00', NULL, '2026-05-28', '2026-06-11', '2026-06-05', 'Devuelto', 1),
(81, 3, 20, 3, '2026-06-27 10:18:00', NULL, 4, '2026-07-06 17:42:00', NULL, '2026-06-27', '2026-07-11', '2026-07-06', 'Devuelto', 1),
(82, 6, 50, 4, '2026-06-27 10:55:00', NULL, 5, '2026-07-07 14:35:00', NULL, '2026-06-27', '2026-07-11', '2026-07-07', 'Devuelto', 1),
(83, 2, 57, 5, '2026-06-27 11:32:00', NULL, 3, '2026-07-04 15:28:00', NULL, '2026-06-27', '2026-07-11', '2026-07-04', 'Devuelto', 1),
(84, 5, 124, 3, '2026-06-27 12:09:00', NULL, 4, '2026-07-05 16:21:00', NULL, '2026-06-27', '2026-07-11', '2026-07-05', 'Devuelto', 1),
(85, 1, 7, 4, '2026-07-27 12:46:00', NULL, 5, '2026-08-05 17:14:00', NULL, '2026-07-27', '2026-08-10', '2026-08-05', 'Devuelto', 1),
(86, 10, 8, 5, '2026-07-27 09:23:00', NULL, 3, '2026-08-06 14:07:00', NULL, '2026-07-27', '2026-08-10', '2026-08-06', 'Devuelto', 1),
(87, 3, 21, 3, '2026-07-27 10:00:00', NULL, 4, '2026-08-03 15:00:00', NULL, '2026-07-27', '2026-08-10', '2026-08-03', 'Devuelto', 1),
(88, 6, 22, 4, '2026-07-27 10:37:00', NULL, 5, '2026-08-04 15:53:00', NULL, '2026-07-27', '2026-08-10', '2026-08-04', 'Devuelto', 1),
(89, 2, 44, 5, '2026-07-27 11:14:00', NULL, 3, '2026-08-05 16:46:00', NULL, '2026-07-27', '2026-08-10', '2026-08-05', 'Devuelto', 1),
(90, 5, 82, 3, '2026-07-27 11:51:00', NULL, 4, '2026-08-06 17:39:00', NULL, '2026-07-27', '2026-08-10', '2026-08-06', 'Devuelto', 1),
(91, 4, 118, 4, '2026-07-27 12:28:00', NULL, 5, '2026-08-03 14:32:00', NULL, '2026-07-27', '2026-08-10', '2026-08-03', 'Devuelto', 1),
(92, 10, 20, 5, '2026-08-21 09:05:00', NULL, 3, '2026-08-29 15:25:00', NULL, '2026-08-21', '2026-09-04', '2026-08-29', 'Devuelto', 1),
(93, 3, 50, 3, '2026-08-21 09:42:00', NULL, 4, '2026-08-30 16:18:00', NULL, '2026-08-21', '2026-09-04', '2026-08-30', 'Devuelto', 1),
(94, 6, 57, 4, '2026-08-21 10:19:00', NULL, 5, '2026-08-31 17:11:00', NULL, '2026-08-21', '2026-09-04', '2026-08-31', 'Devuelto', 1),
(95, 2, 124, 5, '2026-08-21 10:56:00', NULL, 3, '2026-08-28 14:04:00', NULL, '2026-08-21', '2026-09-04', '2026-08-28', 'Devuelto', 1),
(96, 8, 21, 3, '2026-09-07 11:33:00', NULL, 4, '2026-09-15 14:57:00', NULL, '2026-09-07', '2026-09-21', '2026-09-15', 'Devuelto', 1),
(97, 4, 44, 4, '2026-09-07 12:10:00', NULL, 5, '2026-09-16 15:50:00', NULL, '2026-09-07', '2026-09-21', '2026-09-16', 'Devuelto', 1),
(98, 10, 48, 5, '2026-09-07 12:47:00', NULL, 3, '2026-09-17 16:43:00', NULL, '2026-09-07', '2026-09-21', '2026-09-17', 'Devuelto', 1),
(99, 3, 49, 3, '2026-09-07 09:24:00', NULL, 4, '2026-09-14 17:36:00', NULL, '2026-09-07', '2026-09-21', '2026-09-14', 'Devuelto', 1),
(100, 6, 51, 4, '2026-09-07 10:01:00', NULL, 5, '2026-09-15 14:29:00', NULL, '2026-09-07', '2026-09-21', '2026-09-15', 'Devuelto', 1),
(101, 2, 82, 5, '2026-09-07 10:38:00', NULL, 3, '2026-09-16 15:22:00', NULL, '2026-09-07', '2026-09-21', '2026-09-16', 'Devuelto', 1),
(102, 8, 118, 3, '2026-09-07 11:15:00', NULL, 4, '2026-09-17 16:15:00', NULL, '2026-09-07', '2026-09-21', '2026-09-17', 'Devuelto', 1),
(103, 4, 122, 4, '2026-09-07 11:52:00', NULL, 5, '2026-09-14 17:08:00', NULL, '2026-09-07', '2026-09-21', '2026-09-14', 'Devuelto', 1),
(104, 10, 127, 5, '2026-09-07 12:29:00', NULL, 3, '2026-09-15 14:01:00', NULL, '2026-09-07', '2026-09-21', '2026-09-15', 'Devuelto', 1),
(105, 3, 128, 3, '2026-09-07 09:06:00', NULL, 4, '2026-09-16 14:54:00', NULL, '2026-09-07', '2026-09-21', '2026-09-16', 'Devuelto', 1),
(106, 6, 7, 4, '2026-09-21 09:43:00', NULL, 5, '2026-10-01 15:47:00', NULL, '2026-09-21', '2026-10-05', '2026-10-01', 'Devuelto', 1),
(107, 5, 44, 5, '2026-09-21 10:20:00', NULL, 3, '2026-09-28 16:40:00', NULL, '2026-09-21', '2026-10-05', '2026-09-28', 'Devuelto', 1),
(108, 8, 48, 3, '2026-09-21 10:57:00', NULL, 4, '2026-09-29 17:33:00', NULL, '2026-09-21', '2026-10-05', '2026-09-29', 'Devuelto', 1),
(109, 4, 49, 4, '2026-09-21 11:34:00', NULL, 5, '2026-09-30 14:26:00', NULL, '2026-09-21', '2026-10-05', '2026-09-30', 'Devuelto', 1),
(110, 10, 51, 5, '2026-09-21 12:11:00', NULL, 3, '2026-10-01 15:19:00', NULL, '2026-09-21', '2026-10-05', '2026-10-01', 'Devuelto', 1),
(111, 3, 55, 3, '2026-09-21 12:48:00', NULL, 4, '2026-09-28 16:12:00', NULL, '2026-09-21', '2026-10-05', '2026-09-28', 'Devuelto', 1),
(112, 6, 82, 4, '2026-09-21 09:25:00', NULL, 5, '2026-09-29 17:05:00', NULL, '2026-09-21', '2026-10-05', '2026-09-29', 'Devuelto', 1),
(113, 5, 115, 5, '2026-09-21 10:02:00', NULL, 3, '2026-09-30 17:58:00', NULL, '2026-09-21', '2026-10-05', '2026-09-30', 'Devuelto', 1),
(114, 8, 122, 3, '2026-09-21 10:39:00', NULL, 4, '2026-10-01 14:51:00', NULL, '2026-09-21', '2026-10-05', '2026-10-01', 'Devuelto', 1),
(115, 4, 123, 4, '2026-09-21 11:16:00', NULL, 5, '2026-09-28 15:44:00', NULL, '2026-09-21', '2026-10-05', '2026-09-28', 'Devuelto', 1),
(116, 10, 127, 5, '2026-09-21 11:53:00', NULL, 3, '2026-09-29 16:37:00', NULL, '2026-09-21', '2026-10-05', '2026-09-29', 'Devuelto', 1),
(117, 3, 128, 3, '2026-09-21 12:30:00', NULL, 4, '2026-09-30 17:30:00', NULL, '2026-09-21', '2026-10-05', '2026-09-30', 'Devuelto', 1),
(118, 9, 67, 4, '2026-10-07 09:07:00', NULL, 5, '2026-10-17 14:23:00', NULL, '2026-10-07', '2026-10-21', '2026-10-17', 'Devuelto', 1),
(119, 5, 68, 5, '2026-10-07 09:44:00', NULL, 3, '2026-10-14 15:16:00', NULL, '2026-10-07', '2026-10-21', '2026-10-14', 'Devuelto', 1),
(120, 8, 69, 3, '2026-10-07 10:21:00', NULL, 4, '2026-10-15 16:09:00', NULL, '2026-10-07', '2026-10-21', '2026-10-15', 'Devuelto', 1),
(121, 4, 75, 4, '2026-10-07 10:58:00', NULL, 5, '2026-10-16 17:02:00', NULL, '2026-10-07', '2026-10-21', '2026-10-16', 'Devuelto', 1),
(122, 10, 76, 5, '2026-10-07 11:35:00', NULL, 3, '2026-10-17 17:55:00', NULL, '2026-10-07', '2026-10-21', '2026-10-17', 'Devuelto', 1),
(123, 6, 77, 3, '2026-10-07 12:12:00', NULL, 4, '2026-10-14 14:48:00', NULL, '2026-10-07', '2026-10-21', '2026-10-14', 'Devuelto', 1),
(124, 9, 82, 4, '2026-10-07 12:49:00', NULL, 5, '2026-10-15 15:41:00', NULL, '2026-10-07', '2026-10-21', '2026-10-15', 'Devuelto', 1),
(125, 5, 83, 5, '2026-10-07 09:26:00', NULL, 3, '2026-10-16 16:34:00', NULL, '2026-10-07', '2026-10-21', '2026-10-16', 'Devuelto', 1),
(126, 8, 127, 3, '2026-10-07 10:03:00', NULL, 4, '2026-10-17 17:27:00', NULL, '2026-10-07', '2026-10-21', '2026-10-17', 'Devuelto', 1),
(127, 4, 128, 4, '2026-10-07 10:40:00', NULL, 5, '2026-10-14 14:20:00', NULL, '2026-10-07', '2026-10-21', '2026-10-14', 'Devuelto', 1),
(128, 10, 9, 5, '2026-10-23 11:17:00', NULL, 3, '2026-10-31 15:13:00', NULL, '2026-10-23', '2026-11-06', '2026-10-31', 'Devuelto', 1),
(129, 6, 21, 3, '2026-10-23 11:54:00', NULL, 4, '2026-11-01 16:06:00', NULL, '2026-10-23', '2026-11-06', '2026-11-01', 'Devuelto', 1),
(130, 12, 46, 4, '2026-10-23 12:31:00', NULL, 5, '2026-11-02 16:59:00', NULL, '2026-10-23', '2026-11-06', '2026-11-02', 'Devuelto', 1),
(131, 5, 48, 5, '2026-10-23 09:08:00', NULL, 3, '2026-10-30 17:52:00', NULL, '2026-10-23', '2026-11-06', '2026-10-30', 'Devuelto', 1),
(132, 8, 51, 3, '2026-10-23 09:45:00', NULL, 4, '2026-10-31 14:45:00', NULL, '2026-10-23', '2026-11-06', '2026-10-31', 'Devuelto', 1),
(133, 4, 55, 4, '2026-10-23 10:22:00', NULL, 5, '2026-11-01 15:38:00', NULL, '2026-10-23', '2026-11-06', '2026-11-01', 'Devuelto', 1),
(134, 10, 67, 5, '2026-10-23 10:59:00', NULL, 3, '2026-11-02 16:31:00', NULL, '2026-10-23', '2026-11-06', '2026-11-02', 'Devuelto', 1),
(135, 6, 68, 3, '2026-10-23 11:36:00', NULL, 4, '2026-10-30 17:24:00', NULL, '2026-10-23', '2026-11-06', '2026-10-30', 'Devuelto', 1),
(136, 12, 69, 4, '2026-10-23 12:13:00', NULL, 5, '2026-10-31 14:17:00', NULL, '2026-10-23', '2026-11-06', '2026-10-31', 'Devuelto', 1),
(137, 5, 76, 5, '2026-10-23 12:50:00', NULL, 3, '2026-11-01 15:10:00', NULL, '2026-10-23', '2026-11-06', '2026-11-01', 'Devuelto', 1),
(138, 8, 77, 3, '2026-10-23 09:27:00', NULL, 4, '2026-11-02 16:03:00', NULL, '2026-10-23', '2026-11-06', '2026-11-02', 'Devuelto', 1),
(139, 7, 84, 4, '2026-10-23 10:04:00', NULL, 5, '2026-10-30 16:56:00', NULL, '2026-10-23', '2026-11-06', '2026-10-30', 'Devuelto', 1),
(140, 10, 115, 5, '2026-10-23 10:41:00', NULL, 3, '2026-10-31 17:49:00', NULL, '2026-10-23', '2026-11-06', '2026-10-31', 'Devuelto', 1),
(141, 6, 116, 3, '2026-10-23 11:18:00', NULL, 4, '2026-11-01 14:42:00', NULL, '2026-10-23', '2026-11-06', '2026-11-01', 'Devuelto', 1),
(142, 12, 117, 4, '2026-10-23 11:55:00', NULL, 5, '2026-11-02 15:35:00', NULL, '2026-10-23', '2026-11-06', '2026-11-02', 'Devuelto', 1),
(143, 5, 118, 5, '2026-10-23 12:32:00', NULL, 3, '2026-10-30 16:28:00', NULL, '2026-10-23', '2026-11-06', '2026-10-30', 'Devuelto', 1),
(144, 1, 122, 3, '2026-10-23 09:09:00', NULL, 4, '2026-10-31 17:21:00', NULL, '2026-10-23', '2026-11-06', '2026-10-31', 'Devuelto', 1),
(145, 7, 123, 4, '2026-10-23 09:46:00', NULL, 5, '2026-11-01 14:14:00', NULL, '2026-10-23', '2026-11-06', '2026-11-01', 'Devuelto', 1),
(146, 10, 128, 5, '2026-10-23 10:23:00', NULL, 3, '2026-11-02 15:07:00', NULL, '2026-10-23', '2026-11-06', '2026-11-02', 'Devuelto', 1),
(147, 6, 129, 3, '2026-10-23 11:00:00', NULL, 4, '2026-10-30 16:00:00', NULL, '2026-10-23', '2026-11-06', '2026-10-30', 'Devuelto', 1);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `suscripcion`
--

CREATE TABLE `suscripcion` (
  `id` int(11) NOT NULL,
  `id_afiliado` int(11) NOT NULL,
  `fecha_inicio` date NOT NULL,
  `fecha_fin` date NOT NULL,
  `monto` decimal(10,2) NOT NULL,
  `activo` tinyint(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `suscripcion`
--

INSERT INTO `suscripcion` (`id`, `id_afiliado`, `fecha_inicio`, `fecha_fin`, `monto`, `activo`) VALUES
(1, 1, '2026-08-24', '2026-09-22', 8000.00, 1),
(2, 1, '2026-09-23', '2026-10-23', 9000.00, 1),
(3, 1, '2026-10-24', '2026-11-23', 10000.00, 1),
(4, 2, '2026-09-23', '2026-10-23', 9000.00, 1),
(5, 2, '2026-10-24', '2026-11-23', 10000.00, 1),
(6, 3, '2026-10-24', '2026-11-23', 10000.00, 1),
(7, 4, '2026-09-23', '2026-10-23', 9000.00, 1),
(8, 4, '2026-10-24', '2026-11-23', 10000.00, 1),
(9, 5, '2026-10-24', '2026-11-23', 10000.00, 1),
(10, 6, '2026-10-24', '2026-11-23', 10000.00, 1),
(11, 7, '2026-10-24', '2026-11-23', 10000.00, 1),
(12, 8, '2026-10-24', '2026-11-23', 10000.00, 1),
(13, 9, '2026-10-31', '2026-11-30', 10000.00, 1),
(14, 10, '2026-09-04', '2026-10-03', 8000.00, 1),
(15, 12, '2026-10-29', '2026-11-28', 10000.00, 0),
(16, 2, '2026-08-24', '2026-09-22', 8000.00, 1),
(17, 1, '2026-12-23', '2027-01-23', 10000.00, 1);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `usuario`
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
-- Volcado de datos para la tabla `usuario`
--

INSERT INTO `usuario` (`id`, `nombre_usuario`, `nombre`, `apellido`, `email`, `password`, `avatar`, `rol`, `activo`) VALUES
(2, 'admin', 'Administrador', 'Prueba', 'admin@gmail.com', 'AQAAAAIAAYagAAAAEPV4txMaWfVjanZgmDS2s3FWeXW0F1MRrcHDIKhuF3IJn/haSTHxOLuaHstkFAAGwg==', NULL, 1, 1),
(3, 'empleado', 'Jeremy', 'Gilbert', 'empleado@gmail.com', 'AQAAAAIAAYagAAAAEBsC/1PvxL3ygCA5j9Zw88fQXPP9UoKGWbTiENFT48klG0qewyCALouDxvs6lZw8mQ==', '/uploads/avatars/bb8ccdce-9709-4e9a-92c2-ca154de55476_97bee83390ef06ff5275ca1eff82b1d6.jpg', 2, 1),
(4, 'rose.lu', 'Lucía', 'Rosedis', 'lucia.rose@gmail.com', 'AQAAAAIAAYagAAAAEPyzsdFkQj+Btyh5y5KZb6aMTb9haqGNQC8VidrH93wCylNWB9Mk2K/l8neV9kxmog==', '/uploads/avatars/9cdfd81e-9d19-4cad-abc7-96f446992522_2b88dfa2a4960f6fee189146f787426b.jpg', 2, 1),
(5, 'marianatracci', 'Mariana', 'Tracci', 'mariana.t@gmail.com', 'AQAAAAIAAYagAAAAEKA8BIPHukICZGDn7gXYRRC8O1tH84iwRYZg+IoFFOZEMHCAC/ezJM6eevWLG6w9jQ==', NULL, 2, 0),
(6, 'ludmiroklov', 'Ludmila', 'Roklov', 'ludmirok@gmail.com', 'AQAAAAIAAYagAAAAELDDPCbnbkVax62intl15iKejm758YE5IVn4epMyaWLyfnfOQnMkAzfzw0eJ4JDP3Q==', NULL, 1, 1);

--
-- Índices para tablas volcadas
--

--
-- Indices de la tabla `afiliado`
--
ALTER TABLE `afiliado`
  ADD PRIMARY KEY (`id`);

--
-- Indices de la tabla `autor`
--
ALTER TABLE `autor`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_autor_nacionalidad` (`id_nacionalidad`);

--
-- Indices de la tabla `editorial`
--
ALTER TABLE `editorial`
  ADD PRIMARY KEY (`id`);

--
-- Indices de la tabla `ejemplar`
--
ALTER TABLE `ejemplar`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `codigo_inventario` (`codigo_inventario`),
  ADD KEY `fk_ejemplar_libro` (`id_libro`);

--
-- Indices de la tabla `genero`
--
ALTER TABLE `genero`
  ADD PRIMARY KEY (`id`);

--
-- Indices de la tabla `libro`
--
ALTER TABLE `libro`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_libro_autor` (`id_autor`),
  ADD KEY `fk_libro_editorial` (`id_editorial`);

--
-- Indices de la tabla `libro_genero`
--
ALTER TABLE `libro_genero`
  ADD PRIMARY KEY (`id_libro`,`id_genero`),
  ADD KEY `fk_libro_genero_genero` (`id_genero`);

--
-- Indices de la tabla `nacionalidad`
--
ALTER TABLE `nacionalidad`
  ADD PRIMARY KEY (`id`);

--
-- Indices de la tabla `pago`
--
ALTER TABLE `pago`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_pago_afiliado` (`id_afiliado`),
  ADD KEY `fk_pago_usuario_creador` (`creado_por_user_id`),
  ADD KEY `fk_pago_usuario_anulador` (`anulado_por_user_id`),
  ADD KEY `id_suscripcion` (`id_suscripcion`),
  ADD KEY `id_prestamo` (`id_prestamo`);

--
-- Indices de la tabla `prestamo`
--
ALTER TABLE `prestamo`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_prestamo_afiliado` (`id_afiliado`),
  ADD KEY `fk_prestamo_ejemplar` (`id_ejemplar`),
  ADD KEY `fk_prestamo_usuario_creador` (`creado_por_user_id`),
  ADD KEY `fk_prestamo_usuario_anulador` (`anulado_por_user_id`),
  ADD KEY `fk_prestamo_usuario_devolucion` (`devuelto_por_user_id`);

--
-- Indices de la tabla `suscripcion`
--
ALTER TABLE `suscripcion`
  ADD PRIMARY KEY (`id`),
  ADD KEY `id_afiliado` (`id_afiliado`);

--
-- Indices de la tabla `usuario`
--
ALTER TABLE `usuario`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `nombre_usuario` (`nombre_usuario`),
  ADD UNIQUE KEY `email` (`email`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `afiliado`
--
ALTER TABLE `afiliado`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=14;

--
-- AUTO_INCREMENT de la tabla `autor`
--
ALTER TABLE `autor`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=49;

--
-- AUTO_INCREMENT de la tabla `editorial`
--
ALTER TABLE `editorial`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=18;

--
-- AUTO_INCREMENT de la tabla `ejemplar`
--
ALTER TABLE `ejemplar`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=171;

--
-- AUTO_INCREMENT de la tabla `genero`
--
ALTER TABLE `genero`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=14;

--
-- AUTO_INCREMENT de la tabla `libro`
--
ALTER TABLE `libro`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=91;

--
-- AUTO_INCREMENT de la tabla `nacionalidad`
--
ALTER TABLE `nacionalidad`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=14;

--
-- AUTO_INCREMENT de la tabla `pago`
--
ALTER TABLE `pago`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=21;

--
-- AUTO_INCREMENT de la tabla `prestamo`
--
ALTER TABLE `prestamo`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=155;

--
-- AUTO_INCREMENT de la tabla `suscripcion`
--
ALTER TABLE `suscripcion`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=18;

--
-- AUTO_INCREMENT de la tabla `usuario`
--
ALTER TABLE `usuario`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- Restricciones para tablas volcadas
--

--
-- Filtros para la tabla `autor`
--
ALTER TABLE `autor`
  ADD CONSTRAINT `fk_autor_nacionalidad` FOREIGN KEY (`id_nacionalidad`) REFERENCES `nacionalidad` (`id`) ON UPDATE CASCADE;

--
-- Filtros para la tabla `ejemplar`
--
ALTER TABLE `ejemplar`
  ADD CONSTRAINT `fk_ejemplar_libro` FOREIGN KEY (`id_libro`) REFERENCES `libro` (`id`) ON UPDATE CASCADE;

--
-- Filtros para la tabla `libro`
--
ALTER TABLE `libro`
  ADD CONSTRAINT `fk_libro_autor` FOREIGN KEY (`id_autor`) REFERENCES `autor` (`id`) ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_libro_editorial` FOREIGN KEY (`id_editorial`) REFERENCES `editorial` (`id`);

--
-- Filtros para la tabla `libro_genero`
--
ALTER TABLE `libro_genero`
  ADD CONSTRAINT `fk_libro_genero_genero` FOREIGN KEY (`id_genero`) REFERENCES `genero` (`id`) ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_libro_genero_libro` FOREIGN KEY (`id_libro`) REFERENCES `libro` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Filtros para la tabla `pago`
--
ALTER TABLE `pago`
  ADD CONSTRAINT `fk_pago_afiliado` FOREIGN KEY (`id_afiliado`) REFERENCES `afiliado` (`id`) ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_pago_usuario_anulador` FOREIGN KEY (`anulado_por_user_id`) REFERENCES `usuario` (`id`) ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_pago_usuario_creador` FOREIGN KEY (`creado_por_user_id`) REFERENCES `usuario` (`id`) ON UPDATE CASCADE,
  ADD CONSTRAINT `pago_ibfk_1` FOREIGN KEY (`id_suscripcion`) REFERENCES `suscripcion` (`id`),
  ADD CONSTRAINT `pago_ibfk_2` FOREIGN KEY (`id_prestamo`) REFERENCES `prestamo` (`id`);

--
-- Filtros para la tabla `prestamo`
--
ALTER TABLE `prestamo`
  ADD CONSTRAINT `fk_prestamo_afiliado` FOREIGN KEY (`id_afiliado`) REFERENCES `afiliado` (`id`) ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_prestamo_ejemplar` FOREIGN KEY (`id_ejemplar`) REFERENCES `ejemplar` (`id`) ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_prestamo_usuario_anulador` FOREIGN KEY (`anulado_por_user_id`) REFERENCES `usuario` (`id`) ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_prestamo_usuario_creador` FOREIGN KEY (`creado_por_user_id`) REFERENCES `usuario` (`id`) ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_prestamo_usuario_devolucion` FOREIGN KEY (`devuelto_por_user_id`) REFERENCES `usuario` (`id`) ON UPDATE CASCADE;

--
-- Filtros para la tabla `suscripcion`
--
ALTER TABLE `suscripcion`
  ADD CONSTRAINT `suscripcion_ibfk_1` FOREIGN KEY (`id_afiliado`) REFERENCES `afiliado` (`id`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
