using System;
using RedArrow.Jsorm.JsonModels;

namespace RedArrow.Jsorm.Cache
{
    public interface ICacheProvider
    {
        void Register(Type type);

		//Resource Get(Guid id);
    }
}