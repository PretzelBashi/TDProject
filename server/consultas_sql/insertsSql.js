const Inserts = {
    RegistrarUsuario: async (conexion, usuario, constrasena) => {
        try {
            console.log("Creando usuario: " + usuario)
            const [resultados] = await conexion.query(
                "INSERT INTO jugadores (usuario,contrasena,ip,personaje1,personaje2,personaje3)VALUES(?, ?, '', NULL, NULL, NULL);"
                , [usuario, constrasena]);
        } catch (error) {

            console.log(error)

        }
    },

    CrearPersonaje: async (conexion, id, nombre, pelo, cara, color1, boca, clase) => {
        try {
            console.log("Creando personaje: " + nombre)
            const [resultados] = await conexion.query( `
    INSERT INTO personajes (
        id_jugador,
        nombre_personaje,
        pelo,
        cara,
        boca,
        color1,
        color2,
        clase,
        nivel
    )
    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)`,
    [
        id,
        nombre,
        pelo,
        cara,
        boca,
        color1,
        '663300',
        clase,
        1
    ]
);
        } catch (error) {

            console.log(error)

        }
    },
}

module.exports = Inserts;