const { Server } = require("socket.io")
const express = require("express")
const mysql = require("mysql2/promise")
const fs = require("fs")

const utilidades = require("./utilidades")
const IniciarServidor = require("./iniciadorServidor")
const CrearStats = require("./plantillaStats")
const Objetos = require("./objetos")
const listaObjetos = Objetos()
const consultasMysql = require("./consultas_sql/consultasSql")
const insertsMysql = require("./consultas_sql/insertsSql")


const solicitudesHttp = express()

let jugadoresConectados = {}
let usuariosConectados = {}
let servidoresConectados = {}
let usuariosServidoresConectados = {}
let servidores = {
    alpha0: { mundo1: {}, mundo2: {}, mundo3: {} }
}

/*


    */
const baseDeDatos_Jugadores = mysql.createPool({
    host: 'localhost',
    database: 'temporaldreams',
    user: 'root',
    password: 'toor'
})

const servidorSocket = new Server(3000, {
    pingInterval: 25000,
    pingTimeout: 32000,
    cors: {
        origin: "*"
    }
})

console.log("Servidor a la escucha en 3000")


function CargarObjetosInventario(inventario, socket) {
    console.log(listaObjetos[0])
    for (let i = 0; i < inventario.length; i++) {
        console.log(inventario[i].id_objeto)
        jugadoresConectados[socket.id].stats.inventario.push(listaObjetos[inventario[i].id_objeto])
        console.log(jugadoresConectados[socket.id].stats.inventario[i].nombre)
    }
    console.log(jugadoresConectados[socket.id].stats.inventario.length)
}
async function IniciarStatsJugador(socket, usuario, nombre) {
    console.log(jugadoresConectados[socket.id].stats.idJugador + " " + nombre)
    const consulta = await ConsultarBaseDeDatos("consultarPersonaje", "", jugadoresConectados[socket.id].stats.idJugador, nombre)
    const inventario = await ConsultarBaseDeDatos("consultarInventario", "", jugadoresConectados[socket.id].stats.idJugador, nombre)
    CargarObjetosInventario(inventario, socket)

    jugadoresConectados[socket.id].stats.claseJugador = consulta.clase
    jugadoresConectados[socket.id].stats.nivel = consulta.nivel
    jugadoresConectados[socket.id].stats.pelo = consulta.pelo
    jugadoresConectados[socket.id].stats.ojos = consulta.cara
    jugadoresConectados[socket.id].stats.color1 = consulta.color1
    jugadoresConectados[socket.id].stats.ultimaPosicion = { x: consulta.ultimaPosicionx, y: consulta.ultimaPosiciony, z: consulta.ultimaPosicionz }
    jugadoresConectados[socket.id].stats.ultimaRotacion = { x: consulta.ultimaRotacionx, y: consulta.ultimaRotaciony, z: consulta.ultimaRotacionz }
    jugadoresConectados[socket.id].stats.saludMaxima = 100
    jugadoresConectados[socket.id].stats.saludActual = 100
    jugadoresConectados[socket.id].stats.ultimoMundo = "mundo1"
    //Aqui calculalr stats
}

async function ConsultarBaseDeDatos(consulta, usuario, id, nombre, constrasena) {
    let resultadoConsulta
    console.log(consulta + usuario + id + nombre + constrasena)
    switch (consulta) {
        case 'prueba1':
            resultadoConsulta = await consultasMysql.ConsultaPrueba(baseDeDatos_Jugadores)
            break
        case 'consultarJugadorLogin':
            resultadoConsulta = await consultasMysql.ConsultarJugadorLogin(baseDeDatos_Jugadores, usuario, constrasena)
            break
        case 'consultarJugadorRegistro':
            resultadoConsulta = await consultasMysql.ConsultarJugadorRegistro(baseDeDatos_Jugadores, usuario)
            break
        case 'consultarPersonaje':
            resultadoConsulta = await consultasMysql.ConsultarPersonaje(baseDeDatos_Jugadores, id, nombre)
            break
        case 'consultarPersonajes':
            resultadoConsulta = await consultasMysql.ConsultarPersonajes(baseDeDatos_Jugadores, id)
            break
        case 'consultarInventario':
            resultadoConsulta = await consultasMysql.ConsultarInventario(baseDeDatos_Jugadores, id, nombre)
            break
        case 'eliminarPersonaje':
            resultadoConsulta = await consultasMysql.EliminarPersonaje(baseDeDatos_Jugadores, id, nombre)
            break
    } return resultadoConsulta
}

async function InsertarEnBaseDeDatos(consulta, usuario, id, nombre, constrasena) {
    let resultadoConsulta
    switch (consulta) {
        case 'registrarUsuario':
            await insertsMysql.RegistrarUsuario(baseDeDatos_Jugadores, usuario, constrasena)
            break
    } return resultadoConsulta
}

async function CrearPersonaje(id, nombre, pelo, cara, color1, boca, clase) {
    await insertsMysql.CrearPersonaje(baseDeDatos_Jugadores, id, nombre, pelo, cara, color1, boca, clase)
}

async function LoggearJugador(socket, usuario, jugadorConsultado) {

    jugadoresConectados[socket.id] = {
        stats: CrearStats()
    }

    usuariosConectados[usuario] = socket.id

    jugadoresConectados[socket.id].stats.nombreJugador = usuario
    jugadoresConectados[socket.id].stats.idJugador = jugadorConsultado.id
    jugadoresConectados[socket.id].stats.estadoConexion = 1

    let usuarioLogin = {
        nombreJugador: jugadoresConectados[socket.id].stats.nombreJugador,
        PlaceholderPosicion: jugadoresConectados[socket.id].stats.posicionActual,
        saludMaxima: jugadoresConectados[socket.id].stats.saludMaxima,
        saludActual: jugadoresConectados[socket.id].stats.saludActual,
        nivel: jugadoresConectados[socket.id].stats.nivel,
    }


    console.log("DEBUG " + usuariosConectados[usuario])
    console.log("DEBUG " + jugadoresConectados[socket.id].stats.nombreJugador)


    if (usuario == "servidor1mundo1") {
        AsignarEventosClienteServidor(socket)
        console.log("SERVIDOR CONECTADO")
        servidoresConectados[socket.id] = jugadoresConectados[socket.id]
        usuariosServidoresConectados[usuario] = usuariosConectados[usuario]

        servidoresConectados[socket.id].stats.servidorActual = "servidor1"
        servidoresConectados[socket.id].stats.mundoActual = "mundo1"
        servidoresConectados[socket.id].stats.ultimoMundo = "mundo1"
        delete usuariosConectados[usuario]
        delete jugadoresConectados[socket.id]
        socket.emit("loginAdminExitoso", true, "Login exitoso", usuarioLogin)
        return
    }



    socket.emit("loginExitoso", true, "Login exitoso", usuarioLogin)
    LoginExitosoAsignarEventosJugador(socket)
}


servidorSocket.on("connection", (socket) => {
    //Poner aqui desconeccion si el login es incorrecto
    console.log("Jugador conectado:", socket.id)

    const ip = socket.handshake.address;

    console.log("IP del jugador: " + ip);

    socket.on("disconnect", () => {
        try { servidorSocket.to(jugadoresConectados[socket.id].stats.servidorActual + jugadoresConectados[socket.id].stats.mundoActual).emit("jugadorDesconectado", jugadoresConectados[socket.id].stats.nombreJugador) }
        catch { console.log("Jugador no conectado a ningun servidor " + socket.id) }

        try { console.log("Jugador desconectado: " + jugadoresConectados[socket.id].stats.nombreJugador + " Id: " + socket.id) }
        catch { console.log("Jugador desconectado no existe " + socket.id) }

        try { delete servidores[jugadoresConectados[socket.id].stats.servidorActual][jugadoresConectados[socket.id].stats.ultimoMundo][socket.id] }
        catch { console.log("DELETE Jugador en servidor no encontrado " + socket.id) }

        try { delete usuariosConectados[jugadoresConectados[socket.id].stats.nombreJugador] }
        catch { console.log("DELETE Jugador en stats no encontrado " + socket.id) }

        try { delete jugadoresConectados[socket.id] }
        catch { console.log("DELETE Jugador en jugadores no encontrad " + socket.id) }

        if (servidoresConectados[socket.id]) {
            delete usuariosServidoresConectados[servidoresConectados[socket.id].stats.nombreJugador]
            delete servidoresConectados[socket.id]
            console.log("Server manager desconectado " + socket.id)
        }
    });

    socket.on("registro", async (usuario, contrasena) => {
        const usuarioConsultado = await ConsultarBaseDeDatos("consultarJugadorRegistro", usuario, 0, "", "")
        const usuariosEnIp = await ConsultarBaseDeDatos("consultarJugadorRegistro", ip, 0, "", "")
        if (!(usuarioConsultado == null)) {
            if (usuarioConsultado.length > 0) {
                console.log("Usuario ya existe")
                socket.emit("registroExitoso", false, "Ese usuario ya existe")
                socket.disconnect()
                return
            }

        }

        if (!(usuariosEnIp == null)) {
            if (usuariosEnIp.length >= 3) {
                console.log("Usuarios en esta IP:" + usuariosEnIp.length)
                socket.emit("registroExitoso", false, "Ese usuario ya existe")
                socket.disconnect()
                return
            }
        }


        if ((contrasena == "" || contrasena == null) || (usuario == "" || usuario == null) || (usuario == "servidor1mundo1")) {
            console.log("Usuario o contrasena invalida")
            socket.emit("registroExitoso", false, "Usuario o contraseña invalida")
            socket.disconnect()
            return
        }

        InsertarEnBaseDeDatos("registrarUsuario", usuario, '', '', contrasena);
    })

    socket.on("crearPersonaje", async (nombre, pelo, cara, color1, boca, clase) => {
        console.log("Personaje a crear recibido")
        if (nombre == "" || nombre == null) { console.log("Inserte un nombre"); return }

        const personajesConsultados = await ConsultarBaseDeDatos("consultarPersonajes", "", jugadoresConectados[socket.id].stats.idJugador, "")
        if (!(personajesConsultados == null)) {
            if (!(personajesConsultados.length > 0) || personajesConsultados.length < 3) {
                CrearPersonaje(jugadoresConectados[socket.id].stats.idJugador, nombre, pelo, cara, color1, boca, clase)
                socket.emit("personajeCreado", true)
            }
        } else {
            console.log("Nombre de personaje ya existe o esta al maximo de personajes")
            socket.emit("personajeCreado", false)
        }
    })

socket.on("eliminarPersonaje", async (nombre) => {
    console.log("Personaje a eliminar recibido");

    if (nombre == "" || nombre == null) {
        console.log("Inserte un nombre");
        return;
    }

    try {
        const idJugador = jugadoresConectados[socket.id].stats.idJugador;

        const personajesConsultados = await ConsultarBaseDeDatos(
            "consultarPersonaje", "", idJugador, nombre, ""
        );

        if (personajesConsultados != null ) {

            await ConsultarBaseDeDatos(
                "eliminarPersonaje", "", idJugador, nombre, ""
            );

            socket.emit("personajeCreado", true);

        } else {
            console.log("No se encontró el personaje");
            socket.emit("personajeCreado", false);
        }

    } catch (error) {
        console.log("Error al eliminar personaje:", error);
        socket.emit("personajeCreado", false);
    }
});

    socket.on("login", async (usuario, contrasena) => {
        //Aqui va el login y ver si se acepta o se rechaza el intento de coneccion
        //Tambien se deben obtener los datos del jugador (Solo el nombre, dejar todo vacio)
        console.log("Comprobando usuario " + usuariosConectados[usuario])
        const usuarioConsultado = await ConsultarBaseDeDatos("consultarJugadorLogin", usuario, 0, "", contrasena)

        if (usuarioConsultado == null) {
            socket.emit("loginExitoso", false, "Usuario o constraseña incorrecta")
            console.log("Usuario o constraseña incorrecta")
            socket.disconnect()
            return
        }

        if (usuariosServidoresConectados[usuario]) {
            if (servidoresConectados[usuariosServidoresConectados[usuario]].stats.estadoConexion != 2) {
                console.log("Desconexion rechazada")
                console.log(servidoresConectados[usuariosServidoresConectados[usuario]].stats.estadoConexion)
                socket.disconnect()
                return
            }

            socket.emit("loginExitoso", false, "Usuario ya conectado, desconectando cuenta")
            console.log("Servidor ya conectado, desconectando: " + usuariosServidoresConectados[usuario])

            servidorSocket.sockets.sockets.get(usuariosServidoresConectados[usuario]).disconnect()
            socket.disconnect()
            return
        }

        if (usuariosConectados[usuario]) {

            if (jugadoresConectados[usuariosConectados[usuario]].stats.estadoConexion != 2) {
                console.log("Desconexion rechazada")
                console.log(jugadoresConectados[usuariosConectados[usuario]].stats.estadoConexion)
                socket.disconnect()
                return
            }

            socket.emit("loginExitoso", false, "Usuario ya conectado, desconectando cuenta")
            console.log("Jugador ya conectado, desconectando: " + usuariosConectados[usuario])

            servidorSocket.sockets.sockets.get(usuariosConectados[usuario]).disconnect()
            socket.disconnect()
            return
        }

        LoggearJugador(socket, usuario, usuarioConsultado)
    })

});

//Actualizar jugadores en rango
/*
setInterval(() => {
    for (const servidor in servidores) {
        try {
            let listaJugadoresMandar = {}

            for (const id in jugadoresConectados) {

                let usuario = {
                    PlaceholderPosicion: jugadoresConectados[id].stats.posicionActual,
                }
                listaJugadoresMandar[jugadoresConectados[id].stats.nombreJugador] = usuario
            }
            servidorSocket.to(servidor).emit(listaJugadoresMandar)
        } catch { }
    }
}, 2000)
*/

//Enviar posiciones de jugadores
setInterval(() => {
    for (const servidor in servidores) {
        for (const mundo in servidores[servidor])
            try {

                let listaJugadoresMandar = {}

                for (const id in jugadoresConectados) {

                    let usuario = {
                        PlaceholderPosicion: jugadoresConectados[id].stats.posicionActual,
                        PlaceholderRotacion: jugadoresConectados[id].stats.rotacionActual,
                        animacionActual: jugadoresConectados[id].stats.animacionActual
                    }
                    listaJugadoresMandar[jugadoresConectados[id].stats.nombreJugador] = usuario
                }

                servidorSocket.to(mundo).emit("recibirPosicionesJugadores", listaJugadoresMandar)
            } catch { }
    }
}, 70);

setInterval(() => {
    for (const servidor in servidores) {
        for (const mundo in servidores[servidor])
            try {

                let listaJugadoresMandar = {}

                for (const id in jugadoresConectados) {

                    let usuario = {
                        PlaceholderPosicion: jugadoresConectados[id].stats.posicionActual,
                        PlaceholderRotacion: jugadoresConectados[id].stats.rotacionActual,
                        animacionActual: jugadoresConectados[id].stats.animacionActual
                    }
                    listaJugadoresMandar[jugadoresConectados[id].stats.nombreJugador] = usuario
                }

                servidorSocket.to(servidor + mundo).emit("recibirPosicionesJugadores", listaJugadoresMandar)
            } catch { }
    }
}, 70);


function ConseguirVida(id) {
    usuario = {
        saludMaxima: jugadoresConectados[id].stats.saludMaxima,
        saludActual: jugadoresConectados[id].stats.saludActual,
    }
}

function ConseguirNivel(id) {
    usuario = {
        nivel: jugadoresConectados[id].stats.nivel,
    }
}

function AsignarEventosClienteServidor(socket) {
    socket.on("conectarJugadorAInstancia", (servidor, mundo) => { //siempre conecta al ultimo mundo

        if (servidores[servidor]) {
            console.log("Servidor aceptado")
            if (Object.keys(servidores[servidor][servidoresConectados[socket.id].stats.ultimoMundo]).length < 31) {
                socket.join(servidor)
                socket.join(servidor + servidoresConectados[socket.id].stats.ultimoMundo)
                servidoresConectados[socket.id].stats.servidorActual = servidor
                servidoresConectados[socket.id].stats.mundoActual = servidoresConectados[socket.id].stats.ultimoMundo
                socket.emit("conexionAInstanciaExitosa", true)
                console.log("Cantidad de jugadores aceptada")

            } else { socket.emit("conexionAInstanciaExitosa", false) }
        }

    })

    socket.on("solicitarJugadoresConectados", () => {
        if (!servidores[servidoresConectados[socket.id].stats.servidorActual][servidoresConectados[socket.id].stats.mundoActual]) { console.log(servidoresConectados[socket.id].stats.mundoActual); return }

        let listaJugadoresMandar = {}
        console.log("Jugadores enviados")

        for (const id in servidores[servidoresConectados[socket.id].stats.servidorActual][servidoresConectados[socket.id].stats.mundoActual]) {
            let usuario = {
                nombreJugador: jugadoresConectados[id].stats.nombreJugador,
                PlaceholderPosicion: jugadoresConectados[id].stats.posicionActual,
                PlaceholderRotacion: jugadoresConectados[id].stats.rotacionActual,
                saludMaxima: jugadoresConectados[id].stats.saludMaxima,
                saludActual: jugadoresConectados[id].stats.saludActual,
                nivel: jugadoresConectados[id].stats.nivel,
                clase: jugadoresConectados[id].stats.claseJugador,
                animacionActual: jugadoresConectados[id].animacionActual
            }
            listaJugadoresMandar[jugadoresConectados[id].stats.nombreJugador] = usuario
        }

        servidoresConectados[socket.id].stats.estadoConexion = 2
        socket.emit("recibirJugadoresConectados", listaJugadoresMandar)
    })
}

function LoginExitosoAsignarEventosJugador(socket) {
    //PROBAR HABILIDADES
    socket.on("IntentarHabilidad", (habilidad) => {
        statsJugador = jugadoresConectados[socket.id].stats
        let valorNumericoNecesario = 1
        let lanzada = false
        let cooldown = 0

        if (statsJugador.habilidades[habilidad][0] > 0) {
            socket.emit("LanzarHabilidad", false, "", 0, 0)
            return
        }

        switch (habilidad) {
            case "basico":
                switch (statsJugador.stats.claseJugador) {
                    case "espadachin":
                        cooldown = statsJugador.velocidadDeAtaqueActual * 5
                        statsJugador.habilidades[habilidad][0] = cooldown
                        valorNumericoNecesario = statsJugador.danoFisicoActual * (1 + (0.3 * statsJugador.habilidades[habilidad][1]))
                        break
                }
                break
            case "espadaLarga1":
                cooldown = (20) / (1 + (statsJugador.aceleracionCasteoActual / 100))
                statsJugador.habilidades[habilidad][0] = cooldown

                valorNumericoNecesario = statsJugador.danoFisicoActual *
                    (1 + (0.6 * statsJugador.habilidades[habilidad][1]))
                break
        }

        setTimeout(() => {
            statsJugador.habilidades[habilidad][0] = 0
        }, statsJugador.habilidades[habilidad][0] * 1000)

        cooldown = Math.floor(cooldown * 100) / 100;
        socket.emit("LanzarHabilidad", lanzada, habilidad, cooldown,
            utilidades.CalcularCritico(valorNumericoNecesario, statsJugador.probabilidadCriticoActual))
    })

    socket.on("solicitarPersonajes", async () => {
        console.log("perosnajes solicitados " + jugadoresConectados[socket.id].stats.idJugador)
        const consulta = await ConsultarBaseDeDatos("consultarPersonajes", "", jugadoresConectados[socket.id].stats.idJugador, "")
        console.log(jugadoresConectados[socket.id].stats.idJugador)
        let personajesMandar = []
        console.log("Persoajes encontrados: " + consulta.length)
        for (let i = 0; i < consulta.length; i++) {
            consulta[i]
            const personaje = {
                idJugador: consulta[i].id_Jugador,
                nombreJugador: consulta[i].nombre_personaje,
                nivel: consulta[i].nivel,
                pelo: consulta[i].pelo,
                ojos: consulta[i].cara,
                color1: consulta[i].color1
            }
            personajesMandar.push(personaje)

        }
        console.log(personajesMandar.length)
        socket.emit("personajesRecibidos", personajesMandar)
    })

    socket.on("conectarJugadorAInstancia", async (servidor, personaje) => { //siempre conecta al ultimo mundo

        if (servidores[servidor]) {
            console.log("Servidor aceptado")
            if (Object.keys(servidores[servidor][jugadoresConectados[socket.id].stats.ultimoMundo]).length < 31) {
                servidores[servidor][jugadoresConectados[socket.id].stats.ultimoMundo][socket.id] = usuariosConectados[jugadoresConectados[socket.id].stats.nombreJugador]
                socket.join(servidor)
                socket.join(servidor + jugadoresConectados[socket.id].stats.ultimoMundo)
                jugadoresConectados[socket.id].stats.nombrePersonaje = personaje
                jugadoresConectados[socket.id].stats.servidorActual = servidor

                await IniciarStatsJugador(socket, personaje, personaje)

                jugadoresConectados[socket.id].stats.mundoActual = jugadoresConectados[socket.id].stats.ultimoMundo


                let usuario = {
                    nombreJugador: jugadoresConectados[socket.id].stats.nombreJugador,
                    PlaceholderPosicion: jugadoresConectados[socket.id].stats.posicionActual,
                    PlaceholderRotacion: jugadoresConectados[socket.id].stats.rotacionActual,
                    saludMaxima: jugadoresConectados[socket.id].stats.saludMaxima,
                    saludActual: jugadoresConectados[socket.id].stats.saludActual,
                    nivel: jugadoresConectados[socket.id].stats.nivel,
                    clase: jugadoresConectados[socket.id].stats.claseJugador,
                    animacionActual: jugadoresConectados[socket.id].animacionActual,
                    clase: jugadoresConectados[socket.id].stats.clase,
                    boca: jugadoresConectados[socket.id].stats.boca,
                    ojos: jugadoresConectados[socket.id].stats.ojos,
                    pelo: jugadoresConectados[socket.id].stats.pelo,
                    color1: jugadoresConectados[socket.id].stats.color1,
                }
                socket.emit("conexionAInstanciaExitosa", true, usuario)
                servidorSocket.to(servidor + jugadoresConectados[socket.id].stats.mundoActual).emit("jugadorNuevoConectado", usuario)
                console.log("Cantidad de jugadores aceptada")

            } else { socket.emit("conexionAInstanciaExitosa", false, null) }
        }

    })

    socket.on("solicitarJugadoresConectados", () => {
        if (!servidores[jugadoresConectados[socket.id].stats.servidorActual][jugadoresConectados[socket.id].stats.mundoActual]) { console.log(jugadoresConectados[socket.id].stats.mundoActual); return }

        let listaJugadoresMandar = {}
        console.log("Jugadores enviados")

        for (const id in servidores[jugadoresConectados[socket.id].stats.servidorActual][jugadoresConectados[socket.id].stats.mundoActual]) {
            let usuario = {
                nombreJugador: jugadoresConectados[id].stats.nombreJugador,
                PlaceholderPosicion: jugadoresConectados[id].stats.posicionActual,
                PlaceholderRotacion: jugadoresConectados[id].stats.rotacionActual,
                saludMaxima: jugadoresConectados[id].stats.saludMaxima,
                saludActual: jugadoresConectados[id].stats.saludActual,
                nivel: jugadoresConectados[id].stats.nivel,
                clase: jugadoresConectados[id].stats.claseJugador,
                animacionActual: jugadoresConectados[id].animacionActual,
                clase: jugadoresConectados[id].stats.clase,
                boca: jugadoresConectados[id].stats.boca,
                ojos: jugadoresConectados[id].stats.ojos,
                pelo: jugadoresConectados[id].stats.pelo,
                color1: jugadoresConectados[id].stats.color1,
            }
            listaJugadoresMandar[jugadoresConectados[id].stats.nombreJugador] = usuario
        }

        jugadoresConectados[socket.id].stats.estadoConexion = 2
        socket.emit("recibirJugadoresConectados", listaJugadoresMandar)
    })

    socket.on("mandarCoordenadasJugador", (posicion, rotacion, animacion) => {
        try {
            jugadoresConectados[socket.id].stats.posicionActual = posicion
            jugadoresConectados[socket.id].stats.rotacionActual = rotacion
            jugadoresConectados[socket.id].stats.animacionActual = animacion

        } catch { }
    })
    socket.on("solicitarInventario", () => {
        socket.emit("recibirInventario", jugadoresConectados[socket.id].stats.inventario)
    })
    socket.on("chat", (data) => {
        console.log("" + jugadoresConectados[socket.id].stats.nombreJugador + ": " + data)
    })
}

