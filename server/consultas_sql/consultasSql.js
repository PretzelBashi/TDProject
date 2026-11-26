const Consultas = {
    ConsultaPrueba: async (conexion) => {
        try {
            const [resultados] = await conexion.query(
                "SELECT * FROM jugadores"
            );


            //console.log(resultados)

        } catch (error) {

            console.log(error)

        }
    },

    ConsultarJugadorLogin: async (conexion, usuario, constrasena) => {
        try {
            const [resultados] = await conexion.query(
                "SELECT * FROM jugadores WHERE usuario = ? AND contrasena = ?", [usuario, constrasena]
            );

            if (resultados.length <= 0) {
                return null
            }
            return resultados[0]

        } catch (error) {

            console.log(error)

        }

    },

    ConsultarJugadorRegistro: async (conexion, usuario) => {
        try {
            const [resultados] = await conexion.query(
                "SELECT * FROM jugadores WHERE usuario = ?", [usuario]
            );

            if (resultados.length <= 0) {
                return null
            }
            return resultados

        } catch (error) {

            console.log(error)

        }

    },

    ConsultarPersonaje: async (conexion, id, personaje) => {
        try {
            const [resultados] = await conexion.query(
                "SELECT * FROM personajes WHERE id_jugador = ? AND nombre_personaje = ?",
                [id, personaje]
            );

            console.log(resultados)
            return resultados[0]

        } catch (error) {

            console.log(error)

        }
    },

    ConsultarPersonajes: async (conexion, id) => {
        console.log("Personajes consultados")
        try {
            const [resultados] = await conexion.query(
                "SELECT * FROM personajes WHERE id_jugador = ?",
                [id]
            );


            return resultados

        } catch (error) {

            console.log(error)

        }
    },

    ConsultarInventario: async (conexion, id, personaje) => {
        console.log("Personajes consultados")
        try {
            const [resultados] = await conexion.query(
                "SELECT * FROM objetos WHERE id_jugador = ? AND nombre_personaje = ?",
                [id, personaje]
            );

            console.log(resultados)
            return resultados

        } catch (error) {

            console.log(error)

        }
    },

    EliminarPersonaje: async (conexion, id, personaje) => {
        console.log(id);
        console.log(personaje);

        try {
            const [resultados] = await conexion.query(
                "DELETE FROM personajes WHERE id_jugador = ? AND nombre_personaje = ?",
                [id, personaje]
            );

            console.log("Personajes eliminados:", resultados.affectedRows);

        } catch (error) {
            console.log(error);
        }
    },

}

module.exports = Consultas;