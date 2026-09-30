const CalcularCritico = (daño, critico) => {
    let dañoFinal = daño
    if (Math.random() * 100 < critico) {
        dañoFinal += dañoFinal * (critico / 100)
    }
    return (dañoFinal)
}