using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    public enum AuthType
    {
        none,
        ApiKey,
        Bearer,
        OAuth1,
        OAuth2,
        Basic
    }
}
