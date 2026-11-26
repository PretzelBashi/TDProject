INSERT INTO jugadores (
    usuario,
    contrasena,
    ip,
    personaje1,
    personaje2,
    personaje3
)
VALUES
    ('Jugador1', '123456', '', 'Guerrero', 'Mago', 'Arquero'),
    ('Jugador2', 'abcdef', '', 'Paladin', NULL, NULL),
    ('Jugador3', 'qwerty', '', 'Asesino', 'Curandero', NULL),
    ('front', '123', '', 'Pretzel', 'Teto', NULL);
    
SELECT * FROM jugadores;



