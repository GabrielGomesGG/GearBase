namespace GearBase.Interface
{
    /* Duas coisas daqui que não vimos em sala aparecem em todos os DAOs.
       O cmd.LastInsertedId devolve o id que o AUTO_INCREMENT acabou de gerar no INSERT. Ele vem como long,
       por isso o (int) na frente. Precisa dele porque a venda e a OS usam o próprio id logo depois de gravar,
       para atualizar o veículo e lançar no caixa.
       O ExecuteNonQuery devolve quantas linhas o comando mudou. Se for 0 no Update ou no Delete, o id não existe
       e o DAO lança uma exceção avisando.
       O BuscarPorId devolve null quando não acha o registro. */
    public interface IDAO<T>
    {
        int Insert(T objeto);
        void Update(T objeto);
        void Delete(int id);
        List<T> ListarTodos();
        T BuscarPorId(int id);
    }
}
