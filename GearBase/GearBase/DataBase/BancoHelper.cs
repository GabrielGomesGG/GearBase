using MySql.Data.MySqlClient;

namespace GearBase.DataBase
{
    /* Colunas que aceitam NULL no banco.
       O NULL chega no C# como DBNull.Value (não vimos em sala), e o Convert.ToInt32 e os outros Convert
       dão erro com ele, então confiro antes de converter.
       O ? em int?, decimal? e DateOnly? (também não vimos em sala) deixa esses tipos guardarem null,
       igual à coluna do banco. Sem ele, um int sempre tem um número. */
    public static class BancoHelper
    {
        // Leitura (banco -> C#)

        public static string TextoOuNulo(MySqlDataReader reader, string coluna)
        {
            if (reader[coluna] == DBNull.Value)
                return null;
            return reader[coluna].ToString();
        }

        public static int? InteiroOuNulo(MySqlDataReader reader, string coluna)
        {
            if (reader[coluna] == DBNull.Value)
                return null;
            return Convert.ToInt32(reader[coluna]);
        }

        public static decimal? DecimalOuNulo(MySqlDataReader reader, string coluna)
        {
            if (reader[coluna] == DBNull.Value)
                return null;
            return Convert.ToDecimal(reader[coluna]);
        }

        public static DateOnly LerData(MySqlDataReader reader, string coluna)
        {
            return DateOnly.FromDateTime(Convert.ToDateTime(reader[coluna]));
        }

        public static DateOnly? DataOuNula(MySqlDataReader reader, string coluna)
        {
            if (reader[coluna] == DBNull.Value)
                return null;
            return DateOnly.FromDateTime(Convert.ToDateTime(reader[coluna]));
        }

        public static DateTime? DataHoraOuNula(MySqlDataReader reader, string coluna)
        {
            if (reader[coluna] == DBNull.Value)
                return null;
            return Convert.ToDateTime(reader[coluna]);
        }

        // Escrita (C# -> banco)

        // null do C# vira NULL do MySQL
        public static object ParaBanco(object valor)
        {
            if (valor == null)
                return DBNull.Value;
            return valor;
        }

        // DateOnly precisa virar DateTime antes de ir para o MySQL
        public static object ParaBanco(DateOnly? data)
        {
            if (data == null)
                return DBNull.Value;
            DateOnly dia = (DateOnly)data;
            return dia.ToDateTime(TimeOnly.MinValue);
        }
    }
}
