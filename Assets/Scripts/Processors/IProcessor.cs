namespace Assets.Scripts
{
    public interface IProcessor <in TIn, out TOut>
    {
        TOut Process(TIn input);
    }
    public delegate TOut ProcessorDelegate<Tin, TOut>(Tin input);
    public class Combined<A, B, C> : IProcessor<A, C>
    {
        private readonly IProcessor<A, B> first;
        private readonly IProcessor<B, C> second;

        public Combined(IProcessor<A, B> first, IProcessor<B, C> second)
        {
            this.first = first;
            this.second = second;
        }

        public C Process(A input) => second.Process(first.Process(input));
    }
    public class Chain<TIn, TOut>
    {
        private IProcessor<TIn, TOut> processor;

        public Chain(IProcessor<TIn, TOut> processor)
        {
            this.processor = processor;
        }

        public static Chain<TIn, TOut> Start(IProcessor<TIn, TOut> processor)
        {
            return new Chain<TIn, TOut>(processor);
        }

        public Chain<TIn, TNext> Then<TNext>(IProcessor<TOut, TNext> nextProcessor)
        {
            var combined = new Combined<TIn, TOut, TNext>(processor, nextProcessor);
            return new Chain<TIn, TNext>(combined);
        }

        public TOut Run(TIn input) => processor.Process(input);
        public ProcessorDelegate<TIn, TOut> Compile() => input => processor.Process(input);
    }
}
