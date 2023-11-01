﻿namespace configReander
{
    class JWTSettings
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Key { get; set; }
        public int ExpireSeconds { get; set; }
    }
}