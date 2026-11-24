// =====================================================================
//  MINI BANCO 2.0
//  Un solo programa que usa TODOS los temas de la Parte 4 del libro
//  "C# y .NET 10" (Programación Orientada a Objetos).
//
//  Cada tema está marcado así:  [TEMA X]  para que lo encuentres rápido.
// =====================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Xml.Serialization;

namespace MiniBanco;


// =====================================================================
//  [TEMA 9] ENUMS: constantes con nombre
// =====================================================================

// Enum normal: C# le pone los valores 0, 1, 2 automáticamente
public enum TipoMovimiento { Deposito, Retiro, Interes }

// Enum cambiando los valores de las constantes: cada nombre vale lo que yo diga
public enum Comision { Ninguna = 0, Baja = 25, Alta = 50 }

// Para la fábrica de cuentas (patrón Factory Method)
public enum ClaseCuenta { Corriente, Ahorro }


// =====================================================================
//  [TEMA 10] STRUCT: un "paquete" pequeño de datos que se COPIA
// =====================================================================
public struct Movimiento
{
    public TipoMovimiento Tipo;
    public double Monto;
    public string Concepto;

    // Struct con constructor
    public Movimiento(TipoMovimiento tipo, double monto, string concepto)
    {
        Tipo = tipo;
        Monto = monto;
        Concepto = concepto;
    }

    // Struct con método
    public string Describir() => $"{Tipo,-8} RD${Monto,8:0.00}  ({Concepto})";
}


// =====================================================================
//  [TEMA 17] GENÉRICOS: una clase que sirve para CUALQUIER tipo <T>
// =====================================================================
public class Historial<T>
{
    private List<T> elementos = new List<T>();

    public void Agregar(T elemento) => elementos.Add(elemento);
    public int Cantidad => elementos.Count;
    public List<T> Todos() => elementos;
}


// =====================================================================
//  [TEMA 15] DELEGADOS: una variable que guarda un MÉTODO
// =====================================================================
public delegate void Notificacion(string mensaje);                  // ejemplo 1
public delegate double OperacionInteres(double saldo, double tasa); // ejemplo 2


// =====================================================================
//  [TEMA 1-2] CLASE, ATRIBUTOS PRIVADOS, GETTERS Y SETTERS
//  [TEMA 3]   CONSTRUCTORES (sin parámetros, con parámetros,
//             sobrecarga, copia y llamando a otros métodos)
// =====================================================================
public class Cliente
{
    // Atributos PRIVADOS: nadie de afuera los puede tocar directamente
    private string nombre = "";
    private int edad;

    // Getter y setter "largo": permite VALIDAR antes de guardar
    public string Nombre
    {
        get { return nombre; }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                nombre = "Sin nombre";
            else
                nombre = value;
        }
    }

    public int Edad
    {
        get { return edad; }
        set { edad = value < 0 ? 0 : value; }   // no se permiten edades negativas
    }

    // Forma REDUCIDA de getter y setter (C# crea el atributo privado solo)
    public string Correo { get; set; } = "";

    // Otro uso de los getters: un valor CALCULADO, sin setter
    public bool EsMayorDeEdad => Edad >= 18;

    // Constructor SIN parámetros
    public Cliente()
    {
        Nombre = "Sin nombre";
    }

    // Constructor CON parámetros (sobrecarga de constructores)
    public Cliente(string nombre, int edad)
    {
        Nombre = nombre;
        Edad = edad;
        Validar();   // un constructor puede llamar a otros métodos
    }

    // Constructor para COPIAR otro objeto
    public Cliente(Cliente otro)
    {
        Nombre = otro.Nombre;
        Edad = otro.Edad;
        Correo = otro.Correo;
    }

    private void Validar()
    {
        if (!EsMayorDeEdad)
            Console.WriteLine($"  Aviso: {Nombre} es menor de edad.");
    }

    public override string ToString() => $"{Nombre} ({Edad} años) {Correo}";
}


// =====================================================================
//  [TEMA 7] INTERFACES (contratos: "si me implementas, DEBES tener esto")
// =====================================================================
public interface IImprimible
{
    void Imprimir();
}

public interface IReportable
{
    void Imprimir();        // ¡mismo nombre que en IImprimible! (coinciden métodos)
    string Resumen();
}

// Interface para el patrón Composite
public interface IComponenteSaldo
{
    string Nombre { get; }
    double SaldoTotal();
    void MostrarArbol(int nivel);
}


// =====================================================================
//  [TEMA 4] HERENCIA + CLASE ABSTRACTA
//  Cuenta es ABSTRACTA: es el molde base, pero NO se puede hacer new Cuenta()
// =====================================================================
public abstract class Cuenta : IComponenteSaldo
{
    // [TEMA 6] Atributo STATIC: uno solo para toda la clase
    public static int TotalCuentas = 0;

    // PRIVATE: solo esta clase lo ve (ni las hijas)
    private double saldo;

    // PROTECTED: esta clase y sus HIJAS lo ven, pero no el Main
    protected string titular;

    // PUBLIC: todos lo ven
    public int Numero { get; private set; }
    public string Titular => titular;
    public string Nombre => $"#{Numero} {titular}";
    public Historial<Movimiento> Movimientos { get; } = new Historial<Movimiento>();

    // El getter es público, pero el setter es protected (solo hijas pueden cambiarlo)
    public double Saldo
    {
        get => saldo;
        protected set => saldo = value;
    }

    // Delegado: aquí se "conecta" el método que avisará al cliente
    public Notificacion? AlertaSMS;

    // Constructor de la clase madre (las hijas lo llaman con : base(...))
    protected Cuenta(string titular)
    {
        TotalCuentas++;
        Numero = TotalCuentas;
        this.titular = titular;
        RegistroAuditoria.Instancia.Anotar($"Se abrió la cuenta #{Numero} de {titular}");
    }

    // ---------- [TEMA 2] SOBRECARGA DE MÉTODOS ----------
    // Mismo nombre, diferente número de parámetros...
    public void Depositar(double monto) => Depositar(monto, "Depósito");

    public void Depositar(double monto, string concepto)
    {
        Saldo += monto;
        Movimientos.Agregar(new Movimiento(TipoMovimiento.Deposito, monto, concepto));
        Console.WriteLine($"  {titular} depositó RD${monto}. Saldo: RD${Saldo}");
    }

    // ...o diferente TIPO de parámetro (string en vez de double)
    public void Depositar(string montoEnTexto) => Depositar(double.Parse(montoEnTexto), "Depósito en texto");

    // ---------- [TEMA 4] POLIMORFISMO con virtual ----------
    // "virtual" = las hijas PUEDEN cambiar este método con "override"
    public virtual double CalcularComision(double monto) => (double)Comision.Ninguna;

    public bool Retirar(double monto)
    {
        double comision = CalcularComision(monto);   // cada hija calcula la suya
        double total = monto + comision;

        if (total > Saldo)
        {
            AlertaSMS?.Invoke($"Retiro de RD${monto} RECHAZADO a {titular}. Saldo: RD${Saldo}");
            return false;
        }

        Saldo -= total;
        Movimientos.Agregar(new Movimiento(TipoMovimiento.Retiro, total, $"Retiro + comisión RD${comision}"));
        AlertaSMS?.Invoke($"{titular} retiró RD${monto} (comisión RD${comision}). Saldo: RD${Saldo}");
        return true;
    }

    // "abstract" = la madre NO lo escribe; cada hija ESTÁ OBLIGADA a escribirlo
    public abstract string TipoCuenta();

    public virtual void MostrarInfo()
    {
        Console.WriteLine($"  Cuenta #{Numero} | {TipoCuenta(),-9} | {titular,-6} | Saldo: RD${Saldo:0.00}");
    }

    // Para la demostración de "métodos iguales en madre e hija" (sin virtual)
    public void Saludar() => Console.WriteLine("  Hola, soy una Cuenta (método de la MADRE)");

    // Composite: una cuenta es una "hoja" del árbol
    public double SaldoTotal() => Saldo;
    public void MostrarArbol(int nivel) =>
        Console.WriteLine($"{new string(' ', nivel * 4)}- {Nombre}: RD${Saldo:0.00}");

    // Para guardar en JSON / XML
    public ResumenCuenta AResumen() =>
        new ResumenCuenta { Numero = Numero, Titular = titular, Tipo = TipoCuenta(), Saldo = Saldo };
}


// ---------- Hija 1: CuentaCorriente ----------
public class CuentaCorriente : Cuenta
{
    // Herencia y constructores: llama al constructor de la madre
    public CuentaCorriente(string titular) : base(titular) { }

    // Obligatorio porque en la madre es abstract
    public override string TipoCuenta() => "Corriente";

    // Polimorfismo: cambia el comportamiento de la madre
    public override double CalcularComision(double monto) => (double)Comision.Baja;

    // Llamando a métodos de la clase madre con "base."
    public override void MostrarInfo()
    {
        base.MostrarInfo();   // primero hace lo de la madre
        Console.WriteLine($"      (cobra RD${(int)Comision.Baja} por cada retiro)");
    }

    // Método IGUAL al de la madre, pero SIN override: usa "new" (lo oculta)
    public new void Saludar() => Console.WriteLine("  Hola, soy una CuentaCorriente (método de la HIJA)");

    // Esto daría ERROR porque "saldo" es PRIVATE en la madre:
    // public void Trampa() { saldo = 1000000; }

    // Esto SÍ funciona porque "titular" es PROTECTED:
    public string TitularEnMayusculas() => titular.ToUpper();
}


// ---------- Hija 2: CuentaAhorro (SEALED + varias interfaces) ----------
// Herencia múltiple con interfaces: hereda de UNA clase y cumple VARIAS interfaces
public sealed class CuentaAhorro : Cuenta, IImprimible, IReportable
{
    public double Tasa { get; private set; }

    public CuentaAhorro(string titular, double tasa) : base(titular)
    {
        Tasa = tasa;
    }

    public override string TipoCuenta() => "Ahorro";

    // El delegado decide QUÉ fórmula de interés se usa
    public void AplicarInteres(OperacionInteres formula)
    {
        double ganancia = formula(Saldo, Tasa);
        Saldo += ganancia;
        Movimientos.Agregar(new Movimiento(TipoMovimiento.Interes, ganancia, "Interés del mes"));
        Console.WriteLine($"  {titular} ganó RD${ganancia:0.00} de interés. Saldo: RD${Saldo:0.00}");
    }

    // Implementación EXPLÍCITA: las dos interfaces tienen Imprimir(),
    // así que le decimos a C# cuál es cuál
    void IImprimible.Imprimir() =>
        Console.WriteLine($"  [Impresora] Estado de cuenta de {titular}: RD${Saldo:0.00}");

    void IReportable.Imprimir() =>
        Console.WriteLine($"  [Reporte] {Resumen()}");

    public string Resumen() => $"Ahorro de {titular}, tasa {Tasa}%, {Movimientos.Cantidad} movimientos";
}

// Esto daría ERROR porque CuentaAhorro es sealed (evitar la herencia):
// public class CuentaAhorroVIP : CuentaAhorro { }

// Esto daría ERROR porque Cuenta es abstract:
// var c = new Cuenta("Pedro");


// =====================================================================
//  [TEMA 6] CLASES ESTÁTICAS, MÉTODOS ESTÁTICOS Y CONSTRUCTOR STATIC
// =====================================================================
public static class Banco
{
    public static string Nombre;
    public static DateTime Apertura;

    // Constructor static: se ejecuta UNA SOLA VEZ, la primera vez que se usa Banco
    static Banco()
    {
        Nombre = "Banco Reggie";
        Apertura = DateTime.Now;
        Console.WriteLine("  (Se ejecutó el constructor static de Banco)");
    }

    public static void Bienvenida() => Console.WriteLine($"  === Bienvenido a {Nombre} ===");
}

public static class Calculadora
{
    // Dos fórmulas con la MISMA forma (double, double) -> double
    // por eso las dos caben en el delegado OperacionInteres
    public static double InteresSimple(double saldo, double tasa) => saldo * tasa / 100;
    public static double InteresConBono(double saldo, double tasa) => saldo * tasa / 100 + 50;

    // [TEMA 17] Método GENÉRICO: sirve para int, double, string...
    public static T Mayor<T>(T a, T b) where T : IComparable<T> => a.CompareTo(b) >= 0 ? a : b;
}


// =====================================================================
//  [TEMA 13] PATRONES DE DISEÑO
// =====================================================================

// ---------- SINGLETON: una clase que solo puede tener UN objeto ----------
public sealed class RegistroAuditoria
{
    private static RegistroAuditoria? instancia;
    private List<string> anotaciones = new List<string>();

    private RegistroAuditoria() { }   // constructor privado: nadie puede hacer new

    public static RegistroAuditoria Instancia
    {
        get
        {
            if (instancia == null) instancia = new RegistroAuditoria();
            return instancia;
        }
    }

    public void Anotar(string texto) => anotaciones.Add($"{DateTime.Now:HH:mm:ss} {texto}");
    public void MostrarTodo() { foreach (var a in anotaciones) Console.WriteLine($"  {a}"); }
}

// ---------- FACTORY METHOD: un método que FABRICA objetos por ti ----------
public static class FabricaCuentas
{
    public static Cuenta Crear(ClaseCuenta tipo, string titular)
    {
        if (tipo == ClaseCuenta.Ahorro) return new CuentaAhorro(titular, 5);
        return new CuentaCorriente(titular);
    }
}

// ---------- ABSTRACT FACTORY: fábricas de FAMILIAS de objetos ----------
public interface ITarjeta { string Descripcion(); }
public class TarjetaClasica : ITarjeta { public string Descripcion() => "Tarjeta Clásica (límite RD$20,000)"; }
public class TarjetaOro : ITarjeta { public string Descripcion() => "Tarjeta Oro (límite RD$150,000)"; }

public interface IFabricaPaquete
{
    Cuenta CrearCuenta(string titular);
    ITarjeta CrearTarjeta();
}

public class FabricaPaqueteBasico : IFabricaPaquete
{
    public Cuenta CrearCuenta(string titular) => new CuentaCorriente(titular);
    public ITarjeta CrearTarjeta() => new TarjetaClasica();
}

public class FabricaPaquetePremium : IFabricaPaquete
{
    public Cuenta CrearCuenta(string titular) => new CuentaAhorro(titular, 8);
    public ITarjeta CrearTarjeta() => new TarjetaOro();
}

// ---------- BUILDER: construir un objeto paso a paso ----------
public class ClienteBuilder
{
    private Cliente cliente = new Cliente();

    public ClienteBuilder ConNombre(string nombre) { cliente.Nombre = nombre; return this; }
    public ClienteBuilder ConEdad(int edad) { cliente.Edad = edad; return this; }
    public ClienteBuilder ConCorreo(string correo) { cliente.Correo = correo; return this; }
    public Cliente Construir() => cliente;
}

// ---------- ADAPTER: hace que una clase "incompatible" encaje ----------
// Imagina que esta clase viene de otra empresa y NO la podemos cambiar
public class ServicioDolaresExterno
{
    public decimal ConvertirUsdADop(decimal usd) => usd * 60.5m;
}

// Nuestro sistema espera esta interface
public interface IConversorMoneda { double APesos(double monto); }

// El adaptador "traduce" entre los dos
public class AdaptadorDolares : IConversorMoneda
{
    private ServicioDolaresExterno servicio = new ServicioDolaresExterno();
    public double APesos(double monto) => (double)servicio.ConvertirUsdADop((decimal)monto);
}

// ---------- COMPOSITE: tratar un GRUPO igual que a un solo objeto ----------
public class GrupoCuentas : IComponenteSaldo
{
    private List<IComponenteSaldo> miembros = new List<IComponenteSaldo>();
    public string Nombre { get; }

    public GrupoCuentas(string nombre) { Nombre = nombre; }
    public void Agregar(IComponenteSaldo c) => miembros.Add(c);

    public double SaldoTotal()
    {
        double total = 0;
        foreach (var m in miembros) total += m.SaldoTotal();
        return total;
    }

    public void MostrarArbol(int nivel)
    {
        Console.WriteLine($"{new string(' ', nivel * 4)}+ {Nombre} (total RD${SaldoTotal():0.00})");
        foreach (var m in miembros) m.MostrarArbol(nivel + 1);
    }
}

// ---------- FACADE: un solo botón que hace muchas cosas por dentro ----------
public class FachadaBanco
{
    public Cuenta AbrirCuentaCompleta(string nombre, int edad, ClaseCuenta tipo, double depositoInicial)
    {
        Cliente cliente = new ClienteBuilder().ConNombre(nombre).ConEdad(edad).Construir();
        Cuenta cuenta = FabricaCuentas.Crear(tipo, cliente.Nombre);
        cuenta.Depositar(depositoInicial, "Depósito de apertura");
        RegistroAuditoria.Instancia.Anotar($"Fachada: cuenta completa abierta para {nombre}");
        return cuenta;
    }
}

// ---------- MODELO VISTA CONTROLADOR (MVC) ----------
// Modelo = Cuenta (los datos). Vista = cómo se muestran. Controlador = el que coordina.
public interface IVistaCuenta
{
    void MostrarSaldo(string titular, double saldo);
    void MostrarError(string mensaje);
}

public class VistaConsola : IVistaCuenta
{
    public void MostrarSaldo(string titular, double saldo) => Console.WriteLine($"  [Vista] {titular} tiene RD${saldo:0.00}");
    public void MostrarError(string mensaje) => Console.WriteLine($"  [Vista] ERROR: {mensaje}");
}

public class ControladorCuenta
{
    private Cuenta modelo;
    private IVistaCuenta vista;   // depende de una interface, no de una clase concreta (SOLID: D)

    public ControladorCuenta(Cuenta modelo, IVistaCuenta vista)
    {
        this.modelo = modelo;
        this.vista = vista;
    }

    public void Retirar(double monto)
    {
        if (modelo.Retirar(monto)) vista.MostrarSaldo(modelo.Titular, modelo.Saldo);
        else vista.MostrarError("fondos insuficientes");
    }
}


// =====================================================================
//  [TEMA 11] CLASES PARCIALES: una clase escrita en DOS pedazos
// =====================================================================
public partial class Reporte
{
    private List<Cuenta> cuentas;
    public Reporte(List<Cuenta> cuentas) { this.cuentas = cuentas; }
}

public partial class Reporte   // segunda parte (podría estar en otro archivo)
{
    public void Imprimir()
    {
        double total = 0;
        foreach (var c in cuentas) { c.MostrarInfo(); total += c.Saldo; }
        Console.WriteLine($"  Dinero total en el banco: RD${total:0.00}");
    }
}


// =====================================================================
//  [TEMA 12] DESTRUCTOR: se ejecuta cuando el objeto se elimina de memoria
// =====================================================================
public class SesionCajero
{
    private string usuario;

    public SesionCajero(string usuario)
    {
        this.usuario = usuario;
        Console.WriteLine($"  Sesión abierta para {usuario}");
    }

    ~SesionCajero()   // el destructor lleva ~ y el nombre de la clase
    {
        Console.WriteLine($"  Sesión de {usuario} cerrada (lo hizo el destructor)");
    }
}


// =====================================================================
//  [TEMA 16] CLASE PARA GUARDAR EN JSON / XML
//  Debe ser public y tener constructor sin parámetros
// =====================================================================
[Serializable]
public class ResumenCuenta
{
    public int Numero { get; set; }
    public string Titular { get; set; } = "";
    public string Tipo { get; set; } = "";
    public double Saldo { get; set; }
}


// =====================================================================
//  EL PROGRAMA: la historia completa, paso a paso
// =====================================================================
internal class Program
{
    static void Titulo(string texto) => Console.WriteLine($"\n---------- {texto} ----------");

    static void Main(string[] args)
    {
        // ---------------------------------------------------------------
        Titulo("1. Clase estática y constructor static");
        Console.WriteLine("  Todavía no hemos usado Banco...");
        Banco.Bienvenida();   // aquí se ejecuta el constructor static (la primera vez)
        Banco.Bienvenida();   // la segunda vez ya NO se ejecuta
        Console.WriteLine($"  Cuentas abiertas: {Cuenta.TotalCuentas}");

        // ---------------------------------------------------------------
        Titulo("2. Clientes: getters, setters y constructores");
        Cliente juanCliente = new Cliente("Juan", 25);        // constructor con parámetros
        Cliente anonimo = new Cliente();                       // constructor sin parámetros
        Cliente pedro = new Cliente("Pedro", 16);              // llama a Validar() por dentro
        Cliente maria = new Cliente { Nombre = "María", Edad = 30, Correo = "maria@correo.com" }; // usando setters
        Cliente copia = new Cliente(maria);                    // constructor de copia
        copia.Nombre = "";                                     // el setter lo valida
        Console.WriteLine($"  {juanCliente}\n  {anonimo}\n  {pedro}\n  {maria}\n  Copia: {copia}");
        Console.WriteLine($"  ¿Pedro es mayor de edad? {pedro.EsMayorDeEdad}");
        // juanCliente.nombre = "X";   <- ERROR: nombre es privado

        // ---------------------------------------------------------------
        Titulo("3. Herencia: abrir cuentas");
        CuentaCorriente juan = new CuentaCorriente("Juan");
        CuentaAhorro mariaAhorro = new CuentaAhorro("María", 5);
        juan.MostrarInfo();
        mariaAhorro.MostrarInfo();
        Console.WriteLine($"  Cuentas abiertas: {Cuenta.TotalCuentas}");

        // ---------------------------------------------------------------
        Titulo("4. Delegado: conectar las alertas por SMS");
        juan.AlertaSMS = mensaje => Console.WriteLine($"  [SMS] {mensaje}");
        mariaAhorro.AlertaSMS = mensaje => Console.WriteLine($"  [SMS] {mensaje}");
        Console.WriteLine("  Listo: cada retiro enviará un SMS.");

        // ---------------------------------------------------------------
        Titulo("5. Sobrecarga de métodos: tres formas de depositar");
        juan.Depositar(1000);                           // 1 parámetro double
        mariaAhorro.Depositar(2000, "Salario");         // 2 parámetros
        juan.Depositar("500");                          // 1 parámetro string

        // ---------------------------------------------------------------
        Titulo("6. Polimorfismo: retirar (cada cuenta cobra distinto)");
        juan.Retirar(5000);          // rechazado
        juan.Retirar(300);           // Corriente cobra comisión de 25
        mariaAhorro.Retirar(300);    // Ahorro no cobra comisión

        List<Cuenta> todas = new List<Cuenta> { juan, mariaAhorro };
        foreach (Cuenta c in todas)
            Console.WriteLine($"  {c.Titular} tiene una cuenta tipo {c.TipoCuenta()}");

        // ---------------------------------------------------------------
        Titulo("7. Llamar a la madre (base) y métodos iguales (new)");
        juan.MostrarInfo();          // override que usa base.MostrarInfo()
        Cuenta comoMadre = juan;     // la misma cuenta vista como "Cuenta"
        juan.Saludar();              // versión de la HIJA
        comoMadre.Saludar();         // versión de la MADRE (porque se usó new, no override)
        Console.WriteLine($"  Protected en acción: {juan.TitularEnMayusculas()}");

        // ---------------------------------------------------------------
        Titulo("8. Dos variables apuntando al MISMO objeto");
        Cuenta otraVariable = juan;
        otraVariable.Depositar(100);
        Console.WriteLine($"  Saldo visto desde 'juan': RD${juan.Saldo}  <- cambió también");

        // ---------------------------------------------------------------
        Titulo("9. Delegado (2do ejemplo): elegir la fórmula de interés");
        OperacionInteres formula = Calculadora.InteresSimple;
        mariaAhorro.AplicarInteres(formula);
        formula = Calculadora.InteresConBono;   // ahora el delegado guarda OTRO método
        mariaAhorro.AplicarInteres(formula);

        // ---------------------------------------------------------------
        Titulo("10. Interfaces e implementación explícita");
        ((IImprimible)mariaAhorro).Imprimir();
        ((IReportable)mariaAhorro).Imprimir();

        // ---------------------------------------------------------------
        Titulo("11. Enums y structs");
        Console.WriteLine($"  Comisión Baja vale {(int)Comision.Baja}, Alta vale {(int)Comision.Alta}");
        Movimiento m1 = new Movimiento(TipoMovimiento.Deposito, 100, "Prueba");
        Movimiento m2 = m1;   // un struct se COPIA (no es el mismo objeto)
        m2.Monto = 999;
        Console.WriteLine($"  m1.Monto = {m1.Monto} | m2.Monto = {m2.Monto}  <- m1 NO cambió");
        Console.WriteLine("  Movimientos de Juan:");
        foreach (Movimiento m in juan.Movimientos.Todos())
            Console.WriteLine($"    {m.Describir()}");

        // ---------------------------------------------------------------
        Titulo("12. Genéricos");
        Historial<string> notas = new Historial<string>();
        notas.Agregar("Revisar cuenta de Pedro");
        notas.Agregar("Llamar a María");
        Console.WriteLine($"  Historial<Movimiento> de María: {mariaAhorro.Movimientos.Cantidad} elementos");
        Console.WriteLine($"  Historial<string> de notas: {notas.Cantidad} elementos");
        Console.WriteLine($"  Mayor(3, 8) = {Calculadora.Mayor(3, 8)}");
        Console.WriteLine($"  Mayor(juan, maría) = {Calculadora.Mayor(juan.Saldo, mariaAhorro.Saldo)}");
        Console.WriteLine($"  Mayor(\"Ana\", \"Zoe\") = {Calculadora.Mayor("Ana", "Zoe")}");

        // ---------------------------------------------------------------
        Titulo("13. Patrones: Factory Method y Abstract Factory");
        Cuenta luis = FabricaCuentas.Crear(ClaseCuenta.Corriente, "Luis");
        luis.MostrarInfo();
        IFabricaPaquete[] fabricas = { new FabricaPaqueteBasico(), new FabricaPaquetePremium() };
        string[] nombres = { "Ana", "Carlos" };
        List<Cuenta> paquetes = new List<Cuenta>();
        for (int i = 0; i < fabricas.Length; i++)
        {
            Cuenta c = fabricas[i].CrearCuenta(nombres[i]);
            Console.WriteLine($"  {nombres[i]}: cuenta {c.TipoCuenta()} + {fabricas[i].CrearTarjeta().Descripcion()}");
            paquetes.Add(c);
        }

        // ---------------------------------------------------------------
        Titulo("14. Patrones: Builder, Adapter y Facade");
        Cliente sofia = new ClienteBuilder().ConNombre("Sofía").ConEdad(22).ConCorreo("sofia@correo.com").Construir();
        Console.WriteLine($"  Builder creó: {sofia}");

        IConversorMoneda conversor = new AdaptadorDolares();
        double pesos = conversor.APesos(100);
        Console.WriteLine($"  Adapter: US$100 = RD${pesos}");
        luis.Depositar(pesos, "Remesa de US$100");

        FachadaBanco fachada = new FachadaBanco();
        Cuenta sofiaCuenta = fachada.AbrirCuentaCompleta("Sofía", 22, ClaseCuenta.Ahorro, 3000);

        // ---------------------------------------------------------------
        Titulo("15. Patrones: Composite");
        GrupoCuentas hijos = new GrupoCuentas("Hijos");
        hijos.Agregar(luis);
        hijos.Agregar(sofiaCuenta);
        GrupoCuentas familia = new GrupoCuentas("Familia");
        familia.Agregar(juan);
        familia.Agregar(hijos);   // un grupo DENTRO de otro grupo
        familia.MostrarArbol(0);

        // ---------------------------------------------------------------
        Titulo("16. Patrones: Modelo Vista Controlador");
        ControladorCuenta controlador = new ControladorCuenta(luis, new VistaConsola());
        controlador.Retirar(1000);
        controlador.Retirar(99999);

        // ---------------------------------------------------------------
        Titulo("17. Patrones: Singleton (registro de auditoría)");
        Console.WriteLine($"  ¿Es el mismo objeto? {ReferenceEquals(RegistroAuditoria.Instancia, RegistroAuditoria.Instancia)}");
        RegistroAuditoria.Instancia.MostrarTodo();

        // ---------------------------------------------------------------
        Titulo("18. Arreglo de objetos: llenado con ciclo y ordenado");
        Cuenta[] arreglo = new Cuenta[] { juan, mariaAhorro, luis, sofiaCuenta, paquetes[0], paquetes[1] };
        for (int i = 0; i < arreglo.Length; i++)
            if (arreglo[i].Saldo == 0) arreglo[i].Depositar(100 * (i + 1), "Bono de bienvenida");
        Array.Sort(arreglo, (a, b) => b.Saldo.CompareTo(a.Saldo));   // de mayor a menor saldo
        Console.WriteLine("  Ranking por saldo:");
        for (int i = 0; i < arreglo.Length; i++)
            Console.WriteLine($"    {i + 1}. {arreglo[i].Titular,-7} RD${arreglo[i].Saldo:0.00}");

        // ---------------------------------------------------------------
        Titulo("19. Guardar y leer en JSON y XML");
        List<ResumenCuenta> resumen = new List<ResumenCuenta>();
        foreach (Cuenta c in arreglo) resumen.Add(c.AResumen());

        File.WriteAllText("banco.json", JsonSerializer.Serialize(resumen));
        var desdeJson = JsonSerializer.Deserialize<List<ResumenCuenta>>(File.ReadAllText("banco.json"))!;
        Console.WriteLine($"  JSON: se guardaron y leyeron {desdeJson.Count} cuentas. Primera: {desdeJson[0].Titular}");

        var serializador = new XmlSerializer(typeof(List<ResumenCuenta>));
        using (var escritor = new StreamWriter("banco.xml")) serializador.Serialize(escritor, resumen);
        using (var lector = new StreamReader("banco.xml"))
        {
            var desdeXml = (List<ResumenCuenta>)serializador.Deserialize(lector)!;
            Console.WriteLine($"  XML: se guardaron y leyeron {desdeXml.Count} cuentas. Última: {desdeXml[^1].Titular}");
        }

        // ---------------------------------------------------------------
        Titulo("20. Destructor");
        AbrirSesionTemporal();
        GC.Collect();                       // le pedimos a .NET que limpie la memoria
        GC.WaitForPendingFinalizers();      // y esperamos a que corran los destructores

        // ---------------------------------------------------------------
        Titulo("21. Reporte final (clase parcial)");
        Banco.Nombre = "Banco Reggie Internacional";
        Console.WriteLine($"  {Banco.Nombre} | Cuentas abiertas: {Cuenta.TotalCuentas}");
        new Reporte(new List<Cuenta>(arreglo)).Imprimir();
    }

    // Se crea el objeto aquí adentro; al salir del método ya nadie lo usa
    [MethodImpl(MethodImplOptions.NoInlining)]
    static void AbrirSesionTemporal()
    {
        new SesionCajero("cajero01");
    }
}
