using DataPublic;
using FreeRedis;
using GameServerCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Text;

namespace GameServer
{
    public class Startup
    {
        public static RedisClient Redis { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.Configure<TokenManagement>(Configuration.GetSection("tokenConfig"));

            var token = Configuration.GetSection("tokenConfig").Get<TokenManagement>();

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                //Token Validation Parameters
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    //��ȡ������Ҫʹ�õ�Microsoft.IdentityModel.Tokens.SecurityKey����ǩ����֤��
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.
                    GetBytes(token.Secret)),
                    //��ȡ������һ��System.String������ʾ��ʹ�õ���Ч�����߼����ҵķ����ߡ�
                    ValidIssuer = token.Issuer,
                    //��ȡ������һ���ַ��������ַ�����ʾ�����ڼ�����Ч���ڷ������ƵĹ��ڡ�
                    ValidAudience = token.Audience,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                };
            });
            services.AddScoped<IAuthenticateService, TokenAuthenticationService>();
            services.AddScoped<IUserService, UserService>();

            Redis = new RedisClient(Configuration["redis"]);
            Redis.Serialize = obj => JsonConvert.SerializeObject(obj);
            Redis.Deserialize = (json, type) => JsonConvert.DeserializeObject(json, type);

            JsonConvert.DefaultSettings = () => {
                var st = new JsonSerializerSettings();
                st.Converters.Add(new StringEnumConverter());
                st.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                st.DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;
                return st;
            };

            CryptoDecrypt.Initialize();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();

            app.UseServer(new ServerOptions
            {
                Redis = Redis,
                Servers = Configuration["ServerOption:Servers"].Split(";"),
                Server = Configuration["ServerOption:Server"]
            });

            Helper.Initialize(new ClientOptions
            {
                Redis = Redis,
                Servers = Configuration["ServerOption:Servers"].Split(";")
            });

            Helper.Client.OnSend += (s, e) =>
                Console.WriteLine($"Client.SendMessage(server={e.Server},data={JsonConvert.SerializeObject(e.Message)})");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            
        }
    }
}
