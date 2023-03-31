using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace supportsapi.labgenomics.com.Services
{
    public class LabgeDatabase
    {
        /// <summary>
        /// 쿼리를 실행하고 DataTable을 JArray로 반환
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static JArray SqlToJArray(string sql)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LabgeConnection"].ConnectionString);
            conn.Open();

            try
            {
                DataTable dt = new DataTable();

                //SET ARITHABORT ON을 하니 속도가 느려지는 현상이 발생하여 쿼리별로 구문에 추가하는것으로 한다.
                //SqlCommand cmd0 = new SqlCommand("SET ARITHABORT ON", conn);
                //cmd0.ExecuteNonQuery();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.CommandTimeout = 60000;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);

                JArray array = JArray.Parse(JsonConvert.SerializeObject(dt));

                return array;
            }
            finally
            {
                conn.Close();
            }
        }
        /// <summary>
        /// 프로시저 검색 
        /// </summary>
        /// <param name="proname">프로시저명</param>
        /// <param name="sqlParameter">프로시저 파라미터</param>
        /// <returns></returns>
        public static JArray SqlProcedureToJArray(string proname, List<SqlParameter> sqlParameter, string ConnectionString = "")
        {
            SqlConnection conn;
            if (ConnectionString == string.Empty || ConnectionString == "")
            {
                conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LabgeConnection"].ConnectionString);
            }
            else
            {
                conn = new SqlConnection(ConnectionString);
            }
            conn.Open();
            try
            {
                DataTable dt = new DataTable();
                SqlCommand cmd = new SqlCommand(proname, conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(sqlParameter.ToArray());

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);

                JArray array = JArray.Parse(JsonConvert.SerializeObject(dt));
                return array;
            }
            finally
            {
                conn.Close();
            }
        }



        public static JObject SqlToJObject(string sql)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LabgeConnection"].ConnectionString);
            conn.Open();

            try
            {
                DataTable dt = new DataTable();

                //SET ARITHABORT ON을 하니 속도가 느려지는 현상이 발생하여 쿼리별로 구문에 추가하는것으로 한다.
                //SqlCommand cmd0 = new SqlCommand("SET ARITHABORT ON", conn);
                //cmd0.ExecuteNonQuery();
                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
                JObject jObject = new JObject();
                if (dt.Rows.Count > 0)
                {
                    jObject = JObject.Parse(JsonConvert.SerializeObject(dt).Replace("[", string.Empty).Replace("]", string.Empty));
                }

                return jObject;
            }
            finally
            {
                conn.Close();
            }
        }

        /// <summary>
        /// 쿼리 실행
        /// </summary>
        /// <param name="sql"></param>
        public static int ExecuteSql(string sql)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LabgeConnection"].ConnectionString);
            conn.Open();

            try
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                int executeCount = cmd.ExecuteNonQuery();
                return executeCount;
            }
            finally
            {
                conn.Close();
            }
        }

        /// <summary>
        /// Scalar 반환
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static object ExecuteSqlScalar(string sql)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LabgeConnection"].ConnectionString);
            conn.Open();

            try
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                return cmd.ExecuteScalar();
            }
            finally
            {
                conn.Close();
            }
        }

        public static DataTable SqlToDataTable(string sql)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LabgeConnection"].ConnectionString);
            conn.Open();

            try
            {
                DataTable dt = new DataTable();

                //SET ARITHABORT ON을 하니 속도가 느려지는 현상이 발생하여 쿼리별로 구문에 추가하는것으로 한다.
                //SqlCommand cmd0 = new SqlCommand("SET ARITHABORT ON", conn);
                //cmd0.ExecuteNonQuery();
                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);

                return dt;
            }
            finally
            {
                conn.Close();
            }
        }

        /// <summary>
        /// SqlCommand로 실행 (파라메터가 있을 때 사용)
        /// </summary>
        /// <param name="cmd"></param>
        public static void ExecuteSqlCommand(SqlCommand cmd)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LabgeConnection"].ConnectionString);
            conn.Open();

            try
            {
                cmd.Connection = conn;
                cmd.ExecuteNonQuery();
            }
            finally
            {
                conn.Close();
            }
        }
    }
}