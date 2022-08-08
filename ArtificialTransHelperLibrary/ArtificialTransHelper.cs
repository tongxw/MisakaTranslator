﻿using SQLHelperLibrary;
using System;
using System.Collections.Generic;
using System.IO;

namespace ArtificialTransHelperLibrary
{
    public class ArtificialTransHelper
    {
        public SQLHelper sqlite;

        public ArtificialTransHelper(string gameName) {
            if (!Directory.Exists(Environment.CurrentDirectory + "\\ArtificialTranslation"))
                Directory.CreateDirectory(Environment.CurrentDirectory + "\\ArtificialTranslation");


            if (File.Exists(Environment.CurrentDirectory + "\\ArtificialTranslation\\MisakaAT_" + gameName + ".sqlite") == false)
            {
                CreateNewNounTransDB(gameName);
            }
            else
            {
                sqlite = new SQLHelper(Environment.CurrentDirectory + "\\ArtificialTranslation\\MisakaAT_" + gameName + ".sqlite");
            }
        }

        /// <summary>
        /// 添加一条翻译（一般是在游戏过程中机器自动添加翻译）
        /// </summary>
        /// <param name="source"></param>
        /// <param name="Trans"></param>
        /// <returns></returns>
        public int AddTrans(string source,string Trans)
        {
            if (source == null || source == "" || Trans == null) {
                //空条目不添加，且返回假
                return 1;
            }

            //string sql =
            //    $"SELECT * FROM artificialtrans WHERE source = '{source}';";

            //List<string> ret = sqlite.ExecuteReader(sql, 4);

            //if (ret == null) {
            //    return 2;
            //}

            //if (ret.Count >= 3 && ret[2] != "")
            //{
            //    // 源条目存在且机器翻译不为空
            //    return 0;
            //}
            //if (ret.Count >= 3 && Trans == "")
            //{
            //    // 源条目存在且提供的字符串为空
            //    return 0;
            //}
            string sql = string.Empty;
            string oldTrans = getTrans(source);
            if (oldTrans == null)
            {
                // new, insert
                sql = $"INSERT INTO artificialtrans VALUES(NULL,'{source}','{Trans}',NULL);";
            } else
            {
                // exist, update
                sql = $"UPDATE artificialtrans SET machineTrans = '{Trans}' WHERE source = '{source}';";
            }


            //sql =
            //    $"INSERT INTO artificialtrans VALUES(NULL,'{source}','{Trans}',NULL);";
            if (sqlite.ExecuteSql(sql) > 0)
            {
                return 0;
            }
            else
            {
                return 3;
            }
        }

        public string getTrans(string source)
        {
            string sql =
                $"SELECT * FROM artificialtrans WHERE source = '{source}';";

            List<List<String>> ret = sqlite.ExecuteReader(sql, 4);

            if (ret == null || ret.Count == 0)
            {
                return null;
            }


            foreach (List<String> row in ret)
            {
                if (row.Count >= 3)
                {
                    if (row[2] != "")
                    {
                        return row[2];
                    }
                }
            }

            return "";
        }

        public  string getLastError()
        {
            return sqlite.GetLastError();
        }

        /// <summary>
        /// 更新翻译（一般是人为的修改）
        /// </summary>
        /// <param name="source"></param>
        /// <param name="Trans"></param>
        /// <returns></returns>
        public bool UpdateTrans(string source, string Trans)
        {
            string sql =
                $"UPDATE artificialtrans SET userTrans = '{Trans}' WHERE source = '{source}';";
            if (sqlite.ExecuteSql(sql) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 新建一个人工翻译数据库（一个游戏一个库）
        /// </summary>
        /// <param name="gameName"></param>
        private void CreateNewNounTransDB(string gameName)
        {
            SQLHelper.CreateNewDatabase(Environment.CurrentDirectory + "\\ArtificialTranslation\\MisakaAT_" + gameName + ".sqlite");
            sqlite = new SQLHelper(Environment.CurrentDirectory + "\\ArtificialTranslation\\MisakaAT_" + gameName + ".sqlite");
            sqlite.ExecuteSql("CREATE TABLE artificialtrans(id INTEGER PRIMARY KEY AUTOINCREMENT,source TEXT,machineTrans TEXT,userTrans TEXT);");
        }

        /// <summary>
        /// 将数据库内容按格式导出到文件以供他人使用
        /// </summary>
        public static bool ExportDBtoFile(string FilePath,string DBPath) {
            try
            {
                SQLHelper sqliteDB = new SQLHelper(DBPath);

                //让没有直接被定义的用户翻译等于机器翻译
                sqliteDB.ExecuteSql("UPDATE artificialtrans SET userTrans = machineTrans WHERE userTrans is NULL;");

                List<List<string>> ret = sqliteDB.ExecuteReader("SELECT * FROM artificialtrans;", 4);

                FileStream fs = new FileStream(FilePath, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);

                for (int i = 0; i < ret.Count; i++)
                {
                    sw.WriteLine("<j>");
                    sw.WriteLine(ret[i][1]);
                    sw.WriteLine("<c>");
                    sw.WriteLine(ret[i][3]);
                }

                sw.Flush();
                sw.Close();
                fs.Close();
            }
            catch (Exception) {
                return false;
            }
            
            return true;
        }
    }
}
