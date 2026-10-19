const { Server } = require("socket.io")
const express = require("express")
const mysql = require("mysql")
const fs = require("fs")
const utilidades = require("./utilidades")
const CrearStats = require("./plantillaStats")
const { debug } = require("console")

const app = express()

let jugadoresConectados = {}
let usuariosConectados = {}
let servidores = {
    servidor1mundo1: {}
}

/*
Los jugadores se reciben correctamente, hacer lo de recibir posiciones PERO PRIMERO ARREGLAR EL PUTO SALTO, plis
faltaria hacer que los jugadores se instancie

Separar la actualizacion de jugadores conectados (cuando un jugador entra y sale) para actualizar
bien las stats, poner try catch por si el foreach se esta ejecutando y borro algo o algo asi no se

const baseDeDatos_Jugadores = mysql.createConnection({
    host: 'localhost',
    database: 'usuarios_db',
    user: 'root',
    password: ''
})
    */

const servidorSocket = new Server(3000, {
    cors: {
        origin: "*"
    }
})

console.log("Servidor Socket.IO iniciado en el puerto 3000")

servidorSocket.on("connection", (socket) => {
    //Poner aqui desconeccion si el login es incorrecto
    console.log("Jugador conectado:", socket.id)

    socket.on("disconnect", () => {
        try {console.log("Jugador desconectado: " + jugadoresConectados[socket.id].stats.nombreJugador + " Id: " + socket.id)} 
        catch{console.log("Jugador desconectado no existe " + socket.id )}

        try {delete servidores[jugadoresConectados[socket.id].stats.servidorActual][socket.id]} 
        catch {console.log("DELETE Jugador en servidor no encontrado " + socket.id)}

        try {delete usuariosConectados[jugadoresConectados[socket.id].stats.nombreJugador]} 
        catch {console.log("DELETE Jugador en stats no encontrado "  + socket.id)}

        try {delete jugadoresConectados[socket.id]} 
        catch {console.log("DELETE Jugador en jugadores no encontrad "  + socket.id)}
    });

    socket.on("login", (usuario, constrasena) => {
        //Aqui va el login y ver si se acepta o se rechaza el intento de coneccion
        //Tambien se deben obtener los datos del jugador (Solo el nombre, dejar todo vacio)
        console.log("Comprobando usuario " + usuariosConectados[usuario])
        if (usuariosConectados[usuario] ) {
            if(jugadoresConectados[usuariosConectados[usuario]].stats.estadoConexion != 2){
                console.log("Desconexion rechazada")
                console.log(jugadoresConectados[usuariosConectados[usuario]].stats.estadoConexion)
                return
            }
            socket.emit("loginExitoso", false, "Usuario ya conectado, desconectando cuenta")
            console.log("Jugador ya conectado, desconectando: "+ usuariosConectados[usuario])

            servidorSocket.sockets.sockets.get(usuariosConectados[usuario]).disconnect()
            socket.disconnect()
            return
        }

        jugadoresConectados[socket.id] = {
            stats: CrearStats()
        }

        usuariosConectados[usuario] = socket.id
        console.log(usuariosConectados[usuario])

        jugadoresConectados[socket.id].stats.nombreJugador = usuario
        jugadoresConectados[socket.id].stats.posicionActual = jugadoresConectados[socket.id].stats.ultimaPosicion
        jugadoresConectados[socket.id].stats.estadoConexion = 1

        let usuarioLogin = {
            nombreJugador: jugadoresConectados[socket.id].stats.nombreJugador,
            PlaceholderPosicion: jugadoresConectados[socket.id].stats.posicionActual,
            saludMaxima: jugadoresConectados[socket.id].stats.saludMaxima,
            saludActual: jugadoresConectados[socket.id].stats.saludActual,
            nivel: jugadoresConectados[socket.id].stats.nivel,
            clase: jugadoresConectados[socket.id].stats.claseJugador
        }

        console.log("DEBUG " + usuariosConectados[usuario])
        console.log("DEBUG " + jugadoresConectados[socket.id].stats.nombreJugador)

        socket.emit("loginExitoso", true, "Login exitoso", usuarioLogin)
        LoginExitosoAsignarEventosJugador(socket)
    })

});

//Actualizar jugadores en rango
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
            servidorSocket.to(servidor).emit(".", listaJugadoresMandar)
        } catch { }
    }
}, 2000)

//Enviar posiciones de jugadores
setInterval(() => {
    for (const servidor in servidores) {
        try {

            let listaJugadoresMandar = {}

            for (const id in jugadoresConectados) {

                let usuario = {
                    PlaceholderPosicion: jugadoresConectados[id].stats.posicionActual,
                    animacionActual: jugadoresConectados[id].stats.animacionActual
                }
                listaJugadoresMandar[jugadoresConectados[id].stats.nombreJugador] = usuario
            }

            servidorSocket.to(servidor).emit("recibirPosicionesJugadores", listaJugadoresMandar)
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
                switch (statsJugador.stats.clase) {
                    case "espadachin":
                        cooldown = statsJugador.velocidadDeAtaqueActual * 5
                        statsJugador.habilidades[habilidad][0] = cooldown
                        valorNumericoNecesario = statsJugador.danoFisicoActual * (1 + (0.3 * statsJugador.habilidades[habilidad][1]))
                        break
                }
                break
            case "espadaLarga1":
                cooldown = (20) / 1 + (statsJugador.aceleracionCasteoActual / 100)
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

    socket.on("conectarJugadorAInstancia", (servidor) => {
        try{        if (servidores[servidor]) {
            console.log("Servidor aceptado")
            if (Object.keys(servidores[servidor]).length <= 30) {
                servidores[servidor][socket.id] = usuariosConectados[jugadoresConectados[socket.id].stats.nombreJugador]
                socket.join(servidor)
                jugadoresConectados[socket.id].stats.servidorActual = servidor
                socket.emit("conexionAInstanciaExitosa", true)
                console.log("Cantidad de jugadores aceptada")
            }
        } else { socket.emit("conexionAInstanciaExitosa", false) }}
        catch{socket.emit("conexionAInstanciaExitosa", false)}
    })

    socket.on("solicitarJugadoresConectados", () => {
        let listaJugadoresMandar = {}
        console.log("Jugadores enviados")
        for (const id in usuariosConectados) {
            if (servidores[jugadoresConectados[socket.id].stats.servidorActual]) {
                for (const id in servidores[jugadoresConectados[socket.id].stats.servidorActual]) {
                    console.log("Jugador solicitado id: "+id)
                    //if (id == socket.id) { continue }
                    let usuario = {
                        nombreJugador: jugadoresConectados[id].stats.nombreJugador,
                        PlaceholderPosicion: jugadoresConectados[id].stats.posicionActual,
                        saludMaxima: jugadoresConectados[id].stats.saludMaxima,
                        saludActual: jugadoresConectados[id].stats.saludActual,
                        nivel: jugadoresConectados[id].stats.nivel,
                        clase: jugadoresConectados[id].stats.claseJugador,
                        animacionActual: jugadoresConectados[id].animacionActual
                    }
                    listaJugadoresMandar[jugadoresConectados[id].stats.nombreJugador] = usuario
                }
            }
        }
        jugadoresConectados[socket.id].stats.estadoConexion = 2
        socket.emit("recibirJugadoresConectados", listaJugadoresMandar)
    })

    socket.on("mandarCoordenadasJugador", (posicion, animacion) => {
        try{
            jugadoresConectados[socket.id].stats.posicionActual = posicion
            jugadoresConectados[socket.id].stats.animacionActual = animacion
        } catch {}
    })

    socket.on("chat", (data) => {
        console.log("" + jugadoresConectados[socket.id].stats.nombreJugador + ": " + data)
    })
}

