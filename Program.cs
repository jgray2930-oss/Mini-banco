using System;

class Cuenta
{
   
    public static string NombreBanco = "Banco Reggie";
    public static int TotalCuentas = 0;

   
    public int Numero;
    public string Titular;
    public double Saldo;

    
    public Cuenta(string titular)
    {
        TotalCuentas++;         
        Numero = TotalCuentas;   
        Titular = titular;
        Saldo = 0;
    }

    public void Depositar(double monto)
    {
        Saldo = Saldo + monto;
        Console.WriteLine($"  {Titular} depositó RD${monto}. Saldo: RD${Saldo}");
    }

    public void Retirar(double monto)
    {
        if (monto > Saldo)
        {
            Console.WriteLine($"  ❌ {Titular} intentó retirar RD${monto}, pero solo tiene RD${Saldo}.");
            return;   
        }

        Saldo = Saldo - monto;
        Console.WriteLine($"  {Titular} retiró RD${monto}. Saldo: RD${Saldo}");
    }

    public void MostrarInfo()
    {
        Console.WriteLine($"  Cuenta #{Numero} | {Titular} | Saldo: RD${Saldo}");
    }
}



sealed class CuentaAhorro : Cuenta
{
    public double PorcentajeInteres;

    public CuentaAhorro(string titular, double porcentaje) : base(titular)
    {
        PorcentajeInteres = porcentaje;
    }

    public void AplicarInteres()
    {
        double ganancia = Calculadora.Interes(Saldo, PorcentajeInteres);
        Saldo = Saldo + ganancia;
        Console.WriteLine($"  {Titular} ganó RD${ganancia} de interés. Saldo: RD${Saldo}");
    }
}



static class Calculadora
{
    public static double Interes(double saldo, double porcentaje)
    {
        return saldo * porcentaje / 100;
    }
}


class Program
{
    static void Main()
    {
        Console.WriteLine($"=== Bienvenido a {Cuenta.NombreBanco} ===");
        Console.WriteLine($"Cuentas abiertas: {Cuenta.TotalCuentas}\n");

        Console.WriteLine("PASO 1: Juan abre una cuenta normal");
        Cuenta juan = new Cuenta("Juan");
        juan.MostrarInfo();

        Console.WriteLine("\nPASO 2: María abre una cuenta de ahorro (5% de interés)");
        CuentaAhorro maria = new CuentaAhorro("María", 5);
        maria.MostrarInfo();

        Console.WriteLine($"\n→ Cuentas abiertas ahora: {Cuenta.TotalCuentas}");

        Console.WriteLine("\nPASO 3: Ambos depositan dinero");
        juan.Depositar(1000);
        maria.Depositar(2000);   

        Console.WriteLine("\nPASO 4: Juan intenta retirar más de lo que tiene");
        juan.Retirar(5000);

        Console.WriteLine("\nPASO 5: Juan retira una cantidad correcta");
        juan.Retirar(300);

        Console.WriteLine("\nPASO 6: Fin de mes, a María le pagan intereses");
        maria.AplicarInteres();

        Console.WriteLine("\nPASO 7: El banco cambia de nombre (cambia para TODOS)");
        Cuenta.NombreBanco = "Banco Reggie Internacional";

        Console.WriteLine("\n=== RESUMEN FINAL ===");
        Console.WriteLine($"Banco: {Cuenta.NombreBanco}");
        Console.WriteLine($"Total de cuentas: {Cuenta.TotalCuentas}");
        juan.MostrarInfo();
        maria.MostrarInfo();
    }
}
