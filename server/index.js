const { Server } = require("socket.io")
const express = require("express")
const mysql = require("mysql")
const fs = require("fs")
const CrearStats = require("./plantillaStats")

const app = express()

let jugadoresConectados = {}
let usuariosConectados = {}
let servidores = {
    servidor1mundo1: {}
}

/*
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
        
        if(jugadoresConectados[socket.id]){
            console.log("Jugador desconectado: " + jugadoresConectados[socket.id].stats.nombreJugador + " Id: " + socket.id)
            delete usuariosConectados[jugadoresConectados[socket.id].stats.nombreJugador]
            delete jugadoresConectados[socket.id]
        }
    });

    socket.on("login", (usuario, constrasena) => {
        //Aqui va el login y ver si se acepta o se rechaza el intento de coneccion
        //Tambien se deben obtener los datos del jugador (Solo el nombre, dejar todo vacio)
        console.log("Comprobando usuario " + usuariosConectados[usuario])
        if(usuariosConectados[usuario]){
            
            socket.emit("loginExitoso", false, "Usuario ya conectado, desconectando cuenta")

            if(jugadoresConectados[usuariosConectados[usuario]]){
                delete jugadoresConectados[usuariosConectados[usuario]]
            }
            servidorSocket.sockets.sockets.get(usuariosConectados[usuario]).disconnect
            delete usuariosConectados[usuario]

            socket.disconnect
            return
        }

        jugadoresConectados[socket.id] = {
            stats: CrearStats()
        }

        usuariosConectados[usuario] = socket.id
        jugadoresConectados[socket.id].stats.nombreJugador = usuario


        console.log("DEBUG " + usuariosConectados[usuario])
        console.log("DEBUG " + jugadoresConectados[socket.id].stats.nombreJugador)
        
        socket.emit("loginExitoso", true, "Login exitoso")
        LoginExitosoAsignarEventosJugador(socket)
    })

});

setInterval(() => {
    for(const servidor in servidores){
        let listaJugadoresMandar = []

        for (const id in usuariosConectados){
            let usuario = {
                    posicionActual: jugadoresConectados[id].stats.posicionActual,
                    saludMaxima: jugadoresConectados[id].stats.saludMaxima,
                    saludActual: jugadoresConectados[id].stats.saludActual,
                    nivel: jugadoresConectados[id].stats.nivel,
            }
            listaJugadoresMandar.push(usuario)
        }
        servidorSocket.to(servidor).emit(listaJugadoresMandar)
    }
}, 70);

function LoginExitosoAsignarEventosJugador(socket) {
    socket.on("conectarJugadorAInstancia", (servidor) => {
        if (servidores[servidor]) {
            console.log("Servidor aceptado")
            if (Object.keys(servidores[servidor]).length <= 30) {
                servidores[servidor][socket.id] = usuariosConectados[jugadoresConectados[socket.id].stats.nombreJugador]
                socket.join(servidor)
                jugadoresConectados[socket.id].stats.servidorActual = servidor
                socket.emit("conexionAInstanciaExitosa", true)
                console.log("Cantidad de jugadores aceptada")
            }
        } else { socket.emit("conexionAInstanciaExitosa", false) }
    })

    socket.on("SolicitarJugadoresConectados", (jugadorIgnorar) => {

    })

    socket.on("chat", (data) => {
        console.log("" + jugadoresConectados[socket.id].stats.nombreJugador + ": " + data)
    })
}

