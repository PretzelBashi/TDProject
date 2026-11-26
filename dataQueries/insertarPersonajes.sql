INSERT INTO personajes (
    id_jugador,
    nombre_personaje,
    pelo,
    cara,
    color1,
    color2,
    boca,
    clase,
    nivel,
    experiencia,
    ultimaSalud,
    monedas,
    monedasRenacimiento,
    ultimoMundo,
    ultimaPosicionx,
    ultimaPosiciony,
    ultimaPosicionz,
    ultimaRotacionx,
    ultimaRotaciony,
    ultimaRotacionz,
    danoFisico,
    danoMagicoBase,
    armaduraFisica,
    armaduraMagica,
    armaduraCriticos,
    penetracionFisica,
    penetracionMagica,
    aceleracionCasteo,
    velocidadDeAtaque,
    roboVida,
    probabilidadCritico,
    velocidad,
    suerte,
    nivelAgricultura,
    nivelAlquimia,
    nivelCocina,
    nivelSastreria,
    nivelHerrero,
    nivelEscritor,
    nivelCaza,
    nivelPesca,
    nivelMineria,
    nivelForrajeador
)
VALUES
(
    1, 'pedro', 1, 1, '#FF0000', '#000000', 0,
    'Guerrero', 10, 2500, 150,
    500, 10, 'mundo1',
    10.5, 0, 20.5,
    0, 90, 0,
    25, 5, 30, 10, 5, 10, 5, 0, 15, 2, 0.15, 5.5, 10,
    2, 1, 1, 1, 3, 1, 2, 1, 2, 1
),
(
    1, 'mago', 1, 0, '#0000FF', '#FFFFFF', 0,
    'Mago', 15, 5000, 100,
    1200, 25, 'mundo1',
    15.0, 2.5, 30.0,
    0, 180, 0,
    5, 40, 10, 35, 2, 5, 15, 20, 5, 1, 0.25, 4.0, 20,
    1, 3, 2, 1, 1, 2, 1, 3, 1, 2
),
(
    3, 'Arquero1', 1, 0, '#00FF00', '#663300',0,
    'Arquero', 5, 800, 120,
    200, 0, 'mundo1',
    5.0, 0, 15.0,
    0, 45, 0,
    18, 8, 15, 12, 3, 12, 8, 5, 20, 3, 0.20, 6.0, 15,
    1, 1, 1, 1, 1, 1, 2, 1, 1, 3
), 
(
    4, 'pretzel', 1, 1, '00FF00', '663300',1,
    'Arquero', 5, 800, 120,
    200, 0, 'mundo1',
    5.0, 0, 15.0,
    0, 45, 0,
    18, 8, 15, 12, 3, 12, 8, 5, 20, 3, 0.20, 6.0, 15,
    1, 1, 1, 1, 1, 1, 2, 1, 1, 3
),
(
    4, 'teto', 0, 0, '00FF00', '663300',0,
    'Arquero', 5, 800, 120,
    200, 0, 'mundo1',
    5.0, 0, 15.0,
    0, 45, 0,
    18, 8, 15, 12, 3, 12, 8, 5, 20, 3, 0.20, 6.0, 15,
    1, 1, 1, 1, 1, 1, 2, 1, 1, 3
);

SELECT * FROM personajes;

