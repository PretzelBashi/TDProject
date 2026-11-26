create database TemporalDreams;


use TemporalDreams;
CREATE TABLE jugadores (
    id INT AUTO_INCREMENT PRIMARY KEY,
    
    usuario VARCHAR(50) UNIQUE NOT NULL,
    contrasena VARCHAR(255) NOT NULL,
    ip VARCHAR(45) NOT NULL,
    
    fecha_de_registro TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ultimo_login TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    personaje1 VARCHAR(30),
    personaje2 VARCHAR(30),
    personaje3 VARCHAR(30)
);

CREATE TABLE objetos (
	id_jugador INT,
    nombre_personaje VARCHAR(30),
    
    id_objeto INT,
    nivel INT,
    
    encantamiento1_id INT, encantamiento1_nivel INT,
    encantamiento2_id INT, encantamiento2_nivel INT,
    encantamiento3_id INT, encantamiento3_nivel INT,
    encantamiento4_id INT, encantamiento4_nivel INT,
    encantamiento5_id INT, encantamiento5_nivel INT,
    foreign key (id_jugador) references jugadores(id) on delete cascade
);

CREATE TABLE habilidades(
	id_jugador INT,
	nombre_personaje VARCHAR(30),
        
	id_habilidad INT,
    nivel_habilidad INT,
    
    foreign key (id_jugador) references jugadores(id) on delete cascade
);

CREATE TABLE personajes(
		id_jugador INT,
        nombre_personaje VARCHAR(30),
        
        pelo INT,
        cara INT,
        boca INT,
        color1 VARCHAR(16),
        color2 VARCHAR(16),
        
        clase VARCHAR(30),
		nivel INT,
		experiencia INT,
        ultimaSalud INT,
        
		monedas INT,
		monedasRenacimiento INT,
        
        ultimoMundo VARCHAR(30),
		ultimaPosicionx float,
		ultimaPosiciony float,
		ultimaPosicionz float,
		
		ultimaRotacionx float,
		ultimaRotaciony float,
		ultimaRotacionz float,
        
        danoFisico INT,
        danoMagicoBase INT,
        armaduraFisica INT,
        armaduraMagica INT,
        armaduraCriticos INT,
        penetracionFisica INT,
        penetracionMagica INT,
        aceleracionCasteo INT,
        velocidadDeAtaque INT,
        roboVida INT,
        probabilidadCritico float,
        velocidad float,
        suerte INT,
        
		nivelAgricultura INT,
        nivelAlquimia INT,
        nivelCocina INT,
        nivelSastreria INT,
        nivelHerrero INT,
        nivelEscritor INT,
        
		nivelCaza INT,
        nivelPesca INT,
        nivelMineria INT,
        nivelForrajeador INT,
        foreign key (id_jugador) references jugadores(id) on delete cascade
);