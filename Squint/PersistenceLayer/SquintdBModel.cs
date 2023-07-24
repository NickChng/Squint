namespace Squint
{
    using System;
    using System.Windows;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Collections.Generic;
    using System.Collections;
    using System.Xml;
    using System.Xml.Serialization;
    using System.IO;
    using System.Configuration;
    using Npgsql;
    using NpgsqlTypes;
    using Extensions;
    using System.Reflection;
    using Squint.XML_Definitions;
    using Squint.Helpers;

    public class SquintDBModel : DbContext
    {
        public SquintDBModel()
        : base(VersionContextConnection.GetDatabaseConnection(), true) { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("public"); // necessary for postGRESql
            //modelBuilder.Entity<DbConstituent>()
            //        .HasRequired(m => m.DbComponent)
            //        .WithMany(t => t.Constituents)
            //        .HasForeignKey(m => m.ComponentID)
            //        .WillCascadeOnDelete(false);
            //modelBuilder.Entity<DbConstituent>()
            //        .HasRequired(m => m.DbConstituentComponent)
            //        .WithMany(t => t.ConstituentComponents)
            //        .HasForeignKey(m => m.ConstituentCompID)
            //        .WillCascadeOnDelete(false);
            modelBuilder.Entity<DbConstraintResult>()
                .HasRequired(d => d.DbAssessment)
                .WithMany(t => t.ConstraintResults)
                .HasForeignKey(m => m.AssessmentID)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<DbProtocolStructure>()
                .HasRequired(d => d.DbStructureChecklist)
                .WithRequiredPrincipal(p => p.DbProtocolStructure)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<DbConstraintChangelog>()
                .HasRequired(p => p.DbConstraintChangelog_Parent)
                .WithMany()
                .WillCascadeOnDelete(false);


            Database.SetInitializer<SquintDBModel>(new InitializeLookupTables());
            base.OnModelCreating(modelBuilder);
        }
        public class InitializeLookupTables  : CreateDatabaseIfNotExists<SquintDBModel> 
        //public class InitializeLookupTables : DropCreateDatabaseAlways<SquintdBModel>
        {
            protected override void Seed(SquintDBModel context)
            {
                foreach (TreatmentCentres item in Enum.GetValues(typeof(TreatmentCentres)))
                {
                    DbTreatmentCentre TC = new DbTreatmentCentre()
                    {
                        TreatmentCentreName = TypeDisplay.Display(item),
                        TreatmentCentre = (int)item
                    };
                    context.DbTreatmentCentres.Add(TC);
                    context.SaveChanges();
                }
                foreach (Permissions p in Enum.GetValues(typeof(Permissions)))
                {
                    DbPermission PER = new DbPermission()
                    {
                        Name = TypeDisplay.Display(p),
                        PermissionID = (int)p
                    };
                    context.DbPermissions.Add(PER);
                    context.SaveChanges();
                }
                DbPermissionGroup SuperUser = new DbPermissionGroup()
                {
                    ID = 1,
                    PermissionGroupName = "Superuser"
                };
                DbPermissionGroup Dosimetry = new DbPermissionGroup()
                {
                    ID = 2,
                    PermissionGroupName = "Dosimetry",
                };
                SuperUser.Permissions = new List<DbPermission>();
                Dosimetry.Permissions = new List<DbPermission>();
                foreach (DbPermission DbPer in context.DbPermissions)
                {
                    SuperUser.Permissions.Add(DbPer);
                    switch (DbPer.PermissionID)
                    {
                        case (int)Permissions.CanSaveProtocol:
                            Dosimetry.Permissions.Add(DbPer);
                            break;
                        case (int)Permissions.CanSaveAssessment:
                            Dosimetry.Permissions.Add(DbPer);
                            break;
                        case (int)Permissions.CanRetireAssessment:
                            Dosimetry.Permissions.Add(DbPer);
                            break;
                        case (int)Permissions.CanOverwriteAssessment:
                            Dosimetry.Permissions.Add(DbPer);
                            break;
                    }
                }
                context.DbPermissionGroups.Add(SuperUser);
                context.DbPermissionGroups.Add(Dosimetry);
                List<string> Users = new List<string>()
                {
                     "nchng"
                };
                foreach (string S in Users)
                {
                    DbUser U = new DbUser()
                    {
                        ID = 1,
                        ARIA_ID = "nchng",
                        PermissionGroupID = 1, // Superuser, but permisions are not implemented yet
                        FirstName = "Nick",
                        LastName = "Chng"
                    };
                    context.DbUsers.Add(U);
                }
                foreach (Energies item in Enum.GetValues(typeof(Energies)))
                {
                    DbEnergy DbEnergy;
                    switch (item)
                    {
                        case Energies.Unset:
                            DbEnergy = new DbEnergy()
                            {
                                ID = (int)Energies.Unset,
                                Energy = TypeDisplay.Display(Energies.Unset),
                                EnergyString = @"Unset"

                            };
                            context.DbEnergies.Add(DbEnergy);
                            break;
                        case Energies.Item6X:
                            DbEnergy = new DbEnergy()
                            {
                                ID = (int)Energies.Item6X,
                                Energy = TypeDisplay.Display(Energies.Item6X),
                                EnergyString = @"6X"

                            };
                            context.DbEnergies.Add(DbEnergy);
                            break;
                        case Energies.Item10X:
                            DbEnergy = new DbEnergy()
                            {
                                ID = (int)Energies.Item10X,
                                Energy = TypeDisplay.Display(Energies.Item10X),
                                EnergyString = @"10X"
                            };
                            context.DbEnergies.Add(DbEnergy);
                            break;
                        case Energies.Item6XFFF:
                            DbEnergy = new DbEnergy()
                            {
                                ID = (int)Energies.Item6XFFF,
                                Energy = TypeDisplay.Display(Energies.Item6XFFF),
                                EnergyString = @"6XFFF"
                            };
                            context.DbEnergies.Add(DbEnergy);
                            break;
                        case Energies.Item10XFFF:
                            DbEnergy = new DbEnergy()
                            {
                                ID = (int)Energies.Item10XFFF,
                                Energy = TypeDisplay.Display(Energies.Item10XFFF),
                                EnergyString = @"10XFFF"
                            };
                            context.DbEnergies.Add(DbEnergy);
                            break;
                        case Energies.Item15X:
                            DbEnergy = new DbEnergy()
                            {
                                ID = (int)Energies.Item15X,
                                Energy = TypeDisplay.Display(Energies.Item15X),
                                EnergyString = @"15X"
                            };
                            context.DbEnergies.Add(DbEnergy);
                            break;
                    }
                }
                context.SaveChanges();
                foreach (ProtocolTypes item in Enum.GetValues(typeof(ProtocolTypes)))
                {
                    DbProtocolType DbO = new DbProtocolType()
                    {
                        ProtocolTypeName = TypeDisplay.Display(item),
                        ProtocolType = (int)item
                    };
                    context.DbProtocolTypes.Add(DbO);
                }
                foreach (ApprovalLevels item in Enum.GetValues(typeof(ApprovalLevels)))
                {
                    DbApprovalLevel DbO = new DbApprovalLevel()
                    {
                        ApprovalLevelName = TypeDisplay.Display(item),
                        ApprovalLevel = (int)item
                    };
                    context.DbApprovalLevels.Add(DbO);
                }
                foreach (TreatmentSites item in Enum.GetValues(typeof(TreatmentSites)))
                {
                    DbTreatmentSite DbO = new DbTreatmentSite()
                    {
                        TreatmentSiteName = TypeDisplay.Display(item),
                        TreatmentSite = (int)item
                    };
                    context.DbTreatmentSites.Add(DbO);
                }
                foreach (TreatmentIntents item in Enum.GetValues(typeof(TreatmentIntents)))
                {
                    DbTreatmentIntent DbO = new DbTreatmentIntent()
                    {
                        Intent = TypeDisplay.Display(item)
                    };
                    context.DbTreatmentIntents.Add(DbO);
                }
                context.SaveChanges();
                DbStructureLabelGroup SLG = new DbStructureLabelGroup()
                {
                    TreatmentCentreID = 1,
                    GroupDescription = "Default code group"
                };
                context.DbStructureLabelGroups.Add(SLG);
                context.SaveChanges();
                var BeamGeometryDefintionPath = SquintModel.Config.BeamGeometryDefinitions.FirstOrDefault(x => x.Site == SquintModel.Config.Site.CurrentSite);
                if (BeamGeometryDefintionPath != null)
                {
                    try
                    {
                        var BeamGeometryDefintionXMLFile = File.ReadAllText(BeamGeometryDefintionPath.Path);
                        Serializer ser = new Serializer();
                        GeometryDefinitions BeamGeometryDefinitions = ser.Deserialize<GeometryDefinitions>(BeamGeometryDefintionXMLFile);
                        foreach (var G in BeamGeometryDefinitions.Geometry)
                        {
                            TrajectoryTypes T = TrajectoryTypes.Unset;
                            Enum.TryParse(G.Trajectory, out T);
                            DbBeamGeometry DbBG = new DbBeamGeometry()
                            {
                                StartAngle = G.StartAngle,
                                EndAngle = G.EndAngle,
                                EndAngleTolerance = G.EndAngleTolerance,
                                GeometryName = G.GeometryName,
                                StartAngleTolerance = G.StartAngleTolerance,
                                Trajectory = (int)T
                            };
                            context.DbBeamGeometries.Add(DbBG);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Error: Cannot find Beam Geometry Definition file for this site, please check configuration file.  Important: This database will need to be manually dropped from the server so initialization can be re-attempted");
                        throw new Exception();
                    }

                }
                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ValidateOnSaveEnabled = false;
                //Serializer ser = new Serializer();
                //StructureLabelXML StructureListXML = ser.Deserialize<StructureLabelXML>(StructureXMLFile);
                int SLabelID = 1;
                DbStructureLabel SL = new DbStructureLabel() // default "Unset" structure
                {
                    ID = SLabelID++,
                    StructureLabel = "Unset",
                    Designator = SchemaShortNames.SQUINT.Display(),
                    Description = "Squint default structure code",
                    AlphaBetaRatio = 0,
                    StructureType = (int)StructureTypes.Unset,
                    StructurelabelGroupID = SLG.ID
                };
                context.DbStructureLabels.Add(SL);
                context.SaveChanges();
                try // Use PostgreSQL "COPY" command for bulk insert.  EF uses an insert for each entity and this takes a very long time.
                {
                    using (NpgsqlConnection conn = new NpgsqlConnection(VersionContextConnection.ConnectionString()))
                    {
                        conn.Open();
                        string ID = "\"ID\"";
                        string Description = "\"Description\"";
                        string Designator = "\"Designator\"";
                        string AlphaBetaRatio = "\"AlphaBetaRatio\"";
                        string StructureType = "\"StructureType\"";
                        string StructurelabelGroupID = "\"StructurelabelGroupID\"";
                        string StructureLabel = "\"StructureLabel\"";
                        string tableName = "\"DbStructureLabels\"";
                        string Code = "\"Code\"";
                        var StrutureCodePath = SquintModel.Config.StructureCodes.FirstOrDefault(x => x.Site == SquintModel.Config.Site.CurrentSite);
                        if (StrutureCodePath == null)
                        {
                            MessageBox.Show("Error: Cannot find Structure Code file for this site, please check configuration file.  Important: This database will need to be manually dropped from the server so initialization can be re-attempted");
                            throw new Exception();
                        }
                        try
                        {
                            using (var writer = conn.BeginBinaryImport(
                                string.Format("COPY {0} ({1}, {2}, {3}, {4}, {5},{6},{7},{8}) FROM STDIN (FORMAT BINARY)",
                                tableName, ID, StructurelabelGroupID, StructureLabel, AlphaBetaRatio, Description, StructureType, Code, Designator)))
                            {
                                using (StreamReader SR = new StreamReader(StrutureCodePath.Path)) // note that this assumes the CSV is organized as below
                                {
                                    string[] data;
                                    string line;
                                    SR.ReadLine();
                                    while ((line = SR.ReadLine()) != null)
                                    {
                                        data = line.Split(',');
                                        writer.StartRow();
                                        writer.Write(SLabelID++, NpgsqlDbType.Integer); // enter ID
                                        writer.Write(1, NpgsqlDbType.Integer); // StructurelabelGroupID
                                        writer.Write(data[2].Trim(), NpgsqlDbType.Text); // StructureLabel
                                        writer.Write(Convert.ToDouble(data[4]), NpgsqlDbType.Double); // AlphaBetaRatio
                                        writer.Write(data[3].Trim(), NpgsqlDbType.Text); // Description
                                        writer.Write((int)StructureTypes.Unset, NpgsqlDbType.Integer); // StructureType
                                        writer.Write(data[1].Trim(), NpgsqlDbType.Text); // Code
                                        writer.Write(data[0].Trim(), NpgsqlDbType.Text); // Designator
                                    }
                                }
                                writer.Complete();
                            }
                            conn.Close();
                        }
                        catch
                        {
                            MessageBox.Show("Error: Cannot find Structure Code file for this site, please check configuration file.  Important: This database will need to be manually dropped from the server so initialization can be re-attempted");
                            throw new Exception();
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                DbLibraryProtocol RootProtocol = new DbLibraryProtocol
                {
                    ID = 1,
                    AuthorID = 1,
                    ProtocolParentID = 1,
                    ApproverID = 1,
                    DbApprovalLevel = context.DbApprovalLevels.Where(x => x.ApprovalLevel == (int)ApprovalLevels.Unset).Single(),
                    DbTreatmentCentre = context.DbTreatmentCentres.Where(x => x.TreatmentCentre == (int)TreatmentCentres.Unset).Single(),
                    DbTreatmentSite = context.DbTreatmentSites.Where(x => x.TreatmentSite == (int)TreatmentSites.Unset).Single(),
                    DbProtocolType = context.DbProtocolTypes.Where(x => x.ProtocolType == (int)ProtocolTypes.Unset).Single(),
                };
                context.DbLibraryProtocols.Add(RootProtocol);
                context.SaveChanges();
                DbStructureChecklist DbStructureChecklistDefault = new DbStructureChecklist();
                context.DbStructureChecklists.Add(DbStructureChecklistDefault);
                DbProtocolStructure RootStructureID = new DbProtocolStructure
                {
                    ProtocolID = 1,
                    StructureLabelID = 1,
                    DbStructureChecklist = DbStructureChecklistDefault
                };
                context.DbProtocolStructures.Add(RootStructureID);
                context.SaveChanges();
                DbComponent RootComponent = new DbComponent
                {
                    ProtocolID = 1
                };
                context.DbComponents.Add(RootComponent);
                context.SaveChanges();
                DbConstraint RootConstraint = new DbConstraint
                {
                    PrimaryStructureID = 1,
                    ReferenceStructureId = 1,
                    ComponentID = 1,
                };
                context.DbConstraints.Add(RootConstraint);
                context.SaveChanges();
                DbConstraintChangelog RootConstraintLog = new DbConstraintChangelog
                {
                    ConstraintID = 1,
                    ParentLogID = 1,
                    Date = DateTime.Now.ToBinary(),
                    ChangeDescription = "Root"
                };
                context.DbConstraintChangelogs.Add(RootConstraintLog);
                context.SaveChanges();


                DbComponentImaging DbPI = new DbComponentImaging()
                {
                    ID = 1,
                    ComponentID = 1
                };
                context.DbComponentImagings.Add(DbPI);
                DbImaging DbI = new DbImaging()
                {
                    ComponentImagingID = 1,
                    ImagingProtocolName = "Default"
                };
                context.DbImagings.Add(DbI);
                DbProtocolChecklist DbCC = new DbProtocolChecklist()
                {
                    ID = 1,
                    ProtocolID = 1
                };
                context.DbProtocolChecklists.Add(DbCC);
                context.SaveChanges();
                base.Seed(context);
            }
        }
        // Types
        
        public DbSet<DbEnergy> DbEnergies { get; set; }
        // Structures
        public DbSet<DbStructureLabelGroup> DbStructureLabelGroups { get; set; }
        public DbSet<DbStructureLabelException> DbStructureLabelExceptions { get; set; }
        public DbSet<DbStructureLabel> DbStructureLabels { get; set; }
        public DbSet<DbProtocolStructure> DbProtocolStructures { get; set; }
        public DbSet<DbStructureAlias> DbStructureAliases { get; set; }
        // Plan Checks
        public DbSet<DbProtocolChecklist> DbProtocolChecklists { get; set; }
        public DbSet<DbArtifact> DbArtifacts { get; set; }
        public DbSet<DbStructureChecklist> DbStructureChecklists { get; set; }
        // Imaging Protocols
        public DbSet<DbImaging> DbImagings { get; set; }
        public DbSet<DbComponentImaging> DbComponentImagings { get; set; }
        //Protocols
        public DbSet<DbApprovalLevel> DbApprovalLevels { get; set; }
        public DbSet<DbTreatmentSite> DbTreatmentSites { get; set; }
        public DbSet<DbProtocolType> DbProtocolTypes { get; set; }
        public DbSet<DbTreatmentCentre> DbTreatmentCentres { get; set; }
        public DbSet<DbTreatmentIntent> DbTreatmentIntents { get; set; }
        public DbSet<DbCTDeviceId> DbCTDeviceIds { get; set; }
        public DbSet<DbLibraryProtocol> DbLibraryProtocols { get; set; }
        public DbSet<DbSessionProtocol> DbSessionProtocols { get; set; }
        //Session
        public DbSet<DbSession> DbSessions { get; set; }
        public DbSet<DbAssessment> DbAssessments { get; set; }
        public DbSet<DbSessionConstraint> DbSessionConstraints { get; set; }
        public DbSet<DbSessionComponent> DbSessionComponents { get; set; }
        public DbSet<DbSessionProtocolStructure> DbSessionProtocolStructures { get; set; }

        //Component
        public DbSet<DbComponent> DbComponents { get; set; }
        //Beams
        public DbSet<DbBolus> DbBoluses { get; set; }
        public DbSet<DbBeamGeometry> DbBeamGeometries { get; set; }
        public DbSet<DbBeam> DbBeams { get; set; }
        public DbSet<DbBeamAlias> DbBeamAliases { get; set; }
        //Constraints
        public DbSet<DbConstraint> DbConstraints { get; set; }
        public DbSet<DbConstraintChangelog> DbConstraintChangelogs { get; set; }
        public DbSet<DbConstraintResult> DbConstraintResults { get; set; }
        public DbSet<DbConstraintResultCode> DbConstraintResultCodes { get; set; }
        public DbSet<DbUser> DbUsers { get; set; }
        public DbSet<DbPermission> DbPermissions { get; set; }
        public DbSet<DbPermissionGroup> DbPermissionGroups { get; set; }
        //Plans
        public DbSet<DbPlanAssociation> DbPlans { get; set; }
        // Confiug
        
        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.
    }

    //public class DbSiteConfiguration
    //{
    //    [Key]
    //    public int ID { get; set; }
    //    public string Site { get; set; }
    //}
    public class DbStructureLabelGroup // 
    {
        [Key]
        public int ID { get; set; }
        [ForeignKey("DbTreatmentCentre")]
        public int TreatmentCentreID { get; set; }
        public virtual DbTreatmentCentre DbTreatmentCentre { get; set; }
        public string GroupDescription { get; set; }
    }

    public class DbStructureLabel
    {
        [Key]
        public int ID { get; set; }
        [ForeignKey("DbStructureLabelGroup")]
        public int StructurelabelGroupID { get; set; }
        public virtual DbStructureLabelGroup DbStructureLabelGroup { get; set; }
        public string StructureLabel { get; set; }
        public double AlphaBetaRatio { get; set; }
        public string Description { get; set; }
        public int StructureType { get; set; } // target, oar etc.
        public string Code { get; set; }
        public string Designator { get; set; }
        public virtual ICollection<DbProtocolStructure> DbProtocolStructure { get; set; }
        //
        public int? private1 { get; set; }
        public string private2 { get; set; }
    }

    public class DbStructureLabelException
    {
        [Key]
        public int ID { get; set; }
        [ForeignKey("DbStructureLabel")]
        public int StructureLabelID { get; set; }
        public virtual DbStructureLabel DbStructureLabel { get; set; }
        [ForeignKey("DbAssessment")]
        public int AssessmentID { get; set; }
        public virtual DbAssessment DbAssessment { get; set; }
    }

    public class DbTreatmentSite
    {
        [Key]
        public int ID { get; set; }
        public string TreatmentSiteName { get; set; }
        public int TreatmentSite { get; set; }
        public virtual ICollection<DbProtocol> DbProtocols { get; set; }
    }

    public class DbProtocolType
    {
        [Key]
        public int ID { get; set; }
        public string ProtocolTypeName { get; set; }
        public int ProtocolType { get; set; }
        public virtual ICollection<DbProtocol> DbProtocols { get; set; }
    }

    public class DbEnergy
    {
        [Key]
        public int ID { get; set; }
        public string Energy { get; set; }
        public string EnergyString { get; set; }
        public virtual ICollection<DbBeam> DbBeams { get; set; }

    }

    public class DbApprovalLevel
    {
        [Key]
        public int ID { get; set; }
        public string ApprovalLevelName { get; set; }
        public int ApprovalLevel { get; set; }
        public virtual ICollection<DbProtocol> DbProtocols { get; set; }
    }

    public class DbTreatmentCentre
    {
        [Key]
        public int ID { get; set; }
        public string TreatmentCentreName { get; set; }
        public int TreatmentCentre { get; set; }
        public virtual ICollection<DbProtocol> DbProtocols { get; set; }
    }

    public class DbUser
    {
        [Key]
        public int ID { get; set; }
        //FK
        [ForeignKey("DbPermissionGroup")]
        public int PermissionGroupID { get; set; }
        public virtual DbPermissionGroup DbPermissionGroup { get; set; }
        public virtual ICollection<DbProtocol> ProtocolApprovers { get; set; }
        public virtual ICollection<DbProtocol> ProtocolAuthors { get; set; }
        // Properties
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ARIA_ID { get; set; }

    }

    public class DbPermission
    {
        [Key]
        public int ID { get; set; }
        public int PermissionID { get; set; }
        public string Name { get; set; } // name of the permission 
        public string Info { get; set; } // info on the permission
        [InverseProperty("Permissions")]
        public virtual ICollection<DbPermissionGroup> PermissionGroups { get; set; } // navigation to groups that have this right
    }

    public class DbPermissionGroup
    {
        [Key]
        public int ID { get; set; }
        public virtual ICollection<DbUser> DbUser { get; set; }
        [InverseProperty("PermissionGroups")]
        public virtual ICollection<DbPermission> Permissions { get; set; }

        //Properties
        public string PermissionGroupName { get; set; }

    }

    public class DbImaging
    {
        [Key]
        public int ID { get; set; }
        public string ImagingProtocolName { get; set; }
        public int ImagingProtocol { get; set; } // links to an enum
        [ForeignKey("DbComponentImaging")]
        public int ComponentImagingID { get; set; }
        public virtual DbComponentImaging DbComponentImaging { get; set; }
    }

    public class DbComponentImaging
    {
        [Key]
        public int ID { get; set; }
        [ForeignKey("DbComponent")]
        public int ComponentID { get; set; }
        public virtual DbComponent DbComponent { get; set; }
        public virtual ICollection<DbImaging> Imaging { get; set; }
    }

    public class DbBolus
    {
        [Key]
        public int ID { get; set; }

        [ForeignKey("DbBeam")]
        public int BeamId { get; set; }

        public virtual DbBeam DbBeam { get; set; }


        [XmlAttribute]
        public double HU { get; set; }
        [XmlAttribute]
        public double Thickness { get; set; }
        [XmlAttribute]
        public double ToleranceHU { get; set; }
        [XmlAttribute]
        public double ToleranceThickness { get; set; }
        [XmlAttribute]
        public int Indication { get; set; }
    }
    public class DbBeam
    {
        [Key]
        public int ID { get; set; }
        [ForeignKey("DbComponent")]
        public int ComponentID { get; set; }
        public virtual DbComponent DbComponent { get; set; }
        public string ProtocolBeamName { get; set; }
        public int Technique { get; set; }
        public string ToleranceTable { get; set; }
        public double? MinMUWarning { get; set; }
        public double? MaxMUWarning { get; set; }
        public double? MinColRotation { get; set; }
        public double? MaxColRotation { get; set; }
        public double? CouchRotation { get; set; }
        public double? MinX { get; set; }
        public double? MaxX { get; set; }
        public double? MinY { get; set; }
        public double? MaxY { get; set; }
        public int JawTracking_Indication { get; set; }
        public virtual ICollection<DbEnergy> DbEnergies { get; set; }
        public virtual ICollection<DbBolus> DbBoluses { get; set; }
        public virtual ICollection<DbBeamAlias> DbBeamAliases { get; set; }
        public virtual ICollection<DbBeamGeometry> DbBeamGeometries { get; set; }
    }

    public class DbBeamGeometry
    {
        [Key]
        public int ID { get; set; }
        public int Trajectory { get; set; }
        public string GeometryName { get; set; }
        public double StartAngle { get; set; } = -1;
        public double EndAngle { get; set; } = -1;
        public double StartAngleTolerance { get; set; } = 1;
        public double EndAngleTolerance { get; set; } = 1;
        public virtual ICollection<DbBeam> DbBeams { get; set; }
    }

    public class DbTreatmentTechnique
    {
        [Key]
        public int ID { get; set; }
        [ForeignKey("DbComponentChecklist")]
        public int DbProtocolChecklistId { get; set; }
        public virtual DbProtocolChecklist DbProtocolChecklist { get; set; }
        public int TreatmentTechniqueType { get; set; }
        public double MinFields { get; set; }
        public double MaxFields { get; set; }

        public int NumIso { get; set; }
        public bool VMAT_SameStartStop { get; set; }
        public double MinXJaw { get; set; }
        public double MaxXJaw { get; set; }
        public double MinYJaw { get; set; }
        public double MaxYJaw { get; set; }
        public int VMAT_JawTracking { get; set; }
    }

    public class DbTreatmentIntent
    {
        [Key]
        public int ID { get; set; }
        public string Intent { get; set; }
        public virtual ICollection<DbProtocol> Protocols { get; set; }
    }

    public class DbCTDeviceId
    {
        [Key]
        public int ID { get; set; }
        public string CTDeviceId { get; set; }
        public virtual ICollection<DbProtocolChecklist> ProtocolChecklist { get; set; }
    }

    public class DbArtifact
    {
        [Key]
        public int ID { get; set; }
        public double? HU { get; set; }
        public double? ToleranceHU { get; set; }
        [ForeignKey("DbProtocolStructure")]
        public int ProtocolStructure_ID { get; set; }
        public virtual DbProtocolStructure DbProtocolStructure { get; set; }
        [ForeignKey("DbProtocolChecklist")]
        public int DbProtocolChecklistId { get; set; }
        public virtual DbProtocolChecklist DbProtocolChecklist { get; set; }
    }

    public class DbStructureChecklist
    {
        [Key]
        [ForeignKey("DbProtocolStructure")]
        public int ProtocolStructureID { get; set; }
        public virtual DbProtocolStructure DbProtocolStructure { get; set; }
        public bool isPointContourChecked { get; set; } = false;
        public double PointContourThreshold { get; set; }
    }

    public class DbProtocolChecklist
    {
        [Key]
        public int ID { get; set; }
        [ForeignKey("DbProtocol")]
        public int ProtocolID { get; set; }
        public virtual DbProtocol DbProtocol { get; set; }
        public int TreatmentTechniqueType { get; set; }
        //Algorithm
        public int AlgorithmVolumeDose { get; set; }
        public string AirCavityCorrectionVMAT { get; set; } // i.e On / Off
        public string AirCavityCorrectionIMRT { get; set; }
        public int AlgorithmVMATOptimization { get; set; }
        public int AlgorithmIMRTOptimization { get; set; }
        public int FieldNormalizationMode { get; set; }
        public double? AlgorithmResolution { get; set; }
        public bool? HeterogeneityOn { get; set; }
        public double? SliceSpacing { get; set; }
        //Supports
        public int SupportIndication { get; set; }
        public double? CouchSurface { get; set; }
        public double? CouchInterior { get; set; }
        //Artifacts
        public virtual ICollection<DbArtifact> Artifacts { get; set; }
        public virtual ICollection<DbCTDeviceId> CTDeviceIds { get; set; } 

        //Bolus
        public virtual ICollection<DbBolus> Boluses { get; set; }


        // Methods
        public DbProtocolChecklist Clone(DbProtocolChecklist DbO)
        {
            var Clone = (DbProtocolChecklist)this.MemberwiseClone();
            Clone.ID = IDGenerator.GetUniqueId();
            return Clone;
        }
    }

    public class DbProtocol
    {
        [Key]
        public int ID { get; set; }
        //FK
        public virtual ICollection<DbProtocolChecklist> ProtocolChecklists { get; set; }
        public virtual ICollection<DbAssessment> Assessments { get; set; }
        public virtual ICollection<DbComponent> Components { get; set; }

        public virtual ICollection<DbTreatmentIntent> TreatmentIntents { get; set; }
        public virtual ICollection<DbProtocolStructure> ProtocolStructures { get; set; }
        public virtual ICollection<DbSessionProtocol> SessionProtocols { get; set; }
        [ForeignKey("DbTreatmentSite")]
        public int TreatmentSiteID { get; set; } // Author
        public virtual DbTreatmentSite DbTreatmentSite { get; set; }
        [ForeignKey("DbTreatmentCentre")]
        public int TreatmentCentreID { get; set; } // Author
        public virtual DbTreatmentCentre DbTreatmentCentre { get; set; }
        [ForeignKey("DbProtocolType")]
        public int ProtocolTypeID { get; set; } // Author
        public virtual DbProtocolType DbProtocolType { get; set; }
        [ForeignKey("DbUser_ProtocolAuthor")]
        public int AuthorID { get; set; } // Author
        [InverseProperty("ProtocolAuthors")]
        public virtual DbUser DbUser_ProtocolAuthor { get; set; }
        [ForeignKey("DbUser_Approver")]
        public int ApproverID { get; set; } // Author
        [InverseProperty("ProtocolApprovers")]
        public virtual DbUser DbUser_Approver { get; set; }
        [ForeignKey("DbApprovalLevel")]
        public int ApprovalLevelID { get; set; } // Author
        public virtual DbApprovalLevel DbApprovalLevel { get; set; }
        //Properties
        public int ProtocolParentID { get; set; }
        public string ProtocolName { get; set; }
        public string CreationDate { get; set; }
        public long LastModified { get; set; }
        public string RetirementDate { get; set; }
        public string RetiredBy { get; set; }
        public string LastModifiedBy { get; set; }
        public string ApprovalDate { get; set; }
        public bool isRetired { get; set; }
        public string Comments { get; set; }
    }

    public class DbLibraryProtocol : DbProtocol
    {
        // Exists to separate library protocol tables from session protocols for performance
    }
    public class DbSessionProtocol : DbProtocol
    {
        public int ParentProtocolId { get; set; }
        public virtual ICollection<DbSession> DbSession { get; set; }
    }
    public class DbAssessment
    {
        [Key]
        public int ID { get; set; }
        [ForeignKey("DbSession")]
        public int SessionID { get; set; }
        public virtual DbSession DbSession { get; set; }
        public virtual ICollection<DbConstraintResult> ConstraintResults { get; set; }
        //Properties
        public int DisplayOrder { get; set; }
        public string PID { get; set; }
        public string PatientName { get; set; }
        public string SquintUser { get; set; }
        public string DateOfAssessment { get; set; }
        public string AssessmentName { get; set; }
        public string Comments { get; set; }
    }

    public class DbPlanAssociation
    {
        [Key]
        public int ID { get; set; }
        //FK
        [ForeignKey("DbSessionComponent")]
        public int SessionComponentID { get; set; }
        public virtual DbSessionComponent DbSessionComponent { get; set; }
        [ForeignKey("DbAssessment")]
        public int AssessmentID { get; set; }
        public virtual DbAssessment DbAssessment { get; set; }
        [ForeignKey("DbSession")]
        public int SessionId { get; set; }
        public virtual DbSession DbSession { get; set; }
        //Properties
        public string UID { get; set; }
        public string PlanName { get; set; }
        public string CourseName { get; set; }
        public int PlanType { get; set; }
        public long LastModified { get; set; }
        public string LastModifiedBy { get; set; }
    }
    public class DbComponent
    {
        [Key]
        public int ID { get; set; }
        //FK
        [ForeignKey("DbLibraryProtocol")]
        public int ProtocolID { get; set; }
        public virtual DbProtocol DbLibraryProtocol { get; set; }

        public virtual ICollection<DbBeam> DbBeams { get; set; }
        public virtual ICollection<DbSessionComponent> DbSessionComponents { get; set; }
        public virtual ICollection<DbConstraint> Constraints { get; set; }
        public virtual ICollection<DbComponentImaging> ImagingProtocols { get; set; }

        //Properties
        public bool isException { get; set; }
        public int DisplayOrder { get; set; } // the order to display in Squint
        public int ComponentType { get; set; } // phase vs sum
        public int NumFractions { get; set; }
        public int ExceptionType { get; set; }
        public double ReferenceDose { get; set; }
        public string ComponentName { get; set; }

        // Beam Group Parameters
        public int? MinBeams { get; set; }
        public int? MaxBeams { get; set; }
        public int? NumIso { get; set; }
        public double? MinColOffset { get; set; }

        // Prescription
        public double? PrescribedPercentage { get; set; }
        public double? PNVMin { get; set; }
        public double? PNVMax { get; set; }
    }

    public class DbSessionComponent : DbComponent
    {
        [ForeignKey("DbSession")]
        public int SessionID { get; set; }
        public virtual DbSession DbSession { get; set; }
        public int ParentComponentID { get; set; }
    }

    public class DbConstraintResult
    {
        [Key]
        public int ID { get; set; }
        [ForeignKey("DbSessionConstraint")]
        public int SessionConstraintID { get; set; }
        public virtual DbSessionConstraint DbSessionConstraint { get; set; }
        [ForeignKey("DbAssessment")]
        public int AssessmentID { get; set; }
        public virtual DbAssessment DbAssessment { get; set; }
        public virtual ICollection<DbConstraintResultCode> DbConstraintCodes { get; set; }
        //Properties
        public double ResultValue { get; set; }
        //public int StatusCode { get; set; }
        public string ResultString { get; set; }
    }
    public class DbConstraintResultCode
    {
        [Key]
        public int ID { get; set; }
        [ForeignKey("DbConstraintResult")]
        public int ConstraintResultID { get; set; }
        public virtual DbConstraintResult DbConstraintResult { get; set; }
        [ForeignKey("DbSession")]
        public int SessionID { get; set; }
        public virtual DbSession DbSession { get; set; }
        //Data
        public int Code { get; set; }
        public int? private1 { get; set; } // possible future use
        public string private2 { get; set; } // possible future use
    }
    public class DbConstraint
    {
        [Key]
        public int ID { get; set; }
        //FKs
        [ForeignKey("DbComponent")]
        public int ComponentID { get; set; }
        public virtual DbComponent DbComponent { get; set; }
        [ForeignKey("DbProtocolStructure_Primary")]
        public int PrimaryStructureID { get; set; } // the primary structure to which this constraint applies
        public virtual DbProtocolStructure DbProtocolStructure_Primary { get; set; }
        [ForeignKey("DbProtocolStructure_Reference")]
        public int ReferenceStructureId { get; set; } // the primary structure to which this constraint applies
        public virtual DbProtocolStructure DbProtocolStructure_Reference { get; set; }
        public virtual ICollection<DbSessionConstraint> DbSessionConstraints { get; set; }
        public virtual ICollection<DbConstraintChangelog> DbConstraintChangelogs { get; set; }
        //Exception flag
        public int ConstraintType { get; set; } // enum code for constriant type, i.e. V,D, Conformity
        public int DisplayOrder { get; set; } // the order to display in Squint
        public double ConstraintValue { get; set; }
        public int Fractions { get; set; }
        public int ConstraintScale { get; set; }
        public double ReferenceValue { get; set; }
        public int ReferenceType { get; set; } // enum code for upper/lower constraint
        public int ReferenceScale { get; set; }
        // Threshold data
        public double? Stop { get; set; }
        public double? MajorViolation { get; set; }
        public double? MinorViolation { get; set; }
        public string ThresholdDataPath { get; set; }
        public int InterpolationParameterType { get; set; }
        public int InterpolationParameterReference { get; set; }
        // Checklist
    }

    public class DbConstraintChangelog
    {
        [Key]
        public int ID { get; set; }
        [ForeignKey("DbConstraint")]
        public int ConstraintID { get; set; }
        public virtual DbConstraint DbConstraint { get; set; }
        [ForeignKey("DbConstraintChangelog_Parent")]
        public int ParentLogID { get; set; }
        public string ConstraintString { get; set; }
        public virtual DbConstraintChangelog DbConstraintChangelog_Parent { get; set; }
        public long Date { get; set; } // created using DateTime.Now.ToBinary
        public string ChangeDescription { get; set; }
        public string ChangeAuthor { get; set; }
    }

    public class DbSessionConstraint : DbConstraint
    {
        [ForeignKey("DbSession")]
        public int SessionID { get; set; }
        public virtual DbSession DbSession { get; set; }
        public int ParentConstraintID { get; set; }
        public int OriginalNumFractions { get; set; }
        public int OriginalPrimaryStructureID { get; set; }
        public int OriginalSecondaryStructureID { get; set; }
        public double OriginalReferenceValue { get; set; }
        public int OriginalReferenceType { get; set; }
        public int OriginalConstraintType { get; set; }
        public int OriginalConstraintScale { get; set; }
        public int OriginalReferenceScale { get; set; }
        public double OriginalConstraintValue { get; set; }
    }



    //public class DbSessionConThreshold : DbConThreshold
    //{
    //    [ForeignKey("DbSession")]
    //    public int SessionID { get; set; }
    //    public virtual DbSession DbSession { get; set; }
    //    public int ParentConstraintThresholdID { get; set; }
    //}

    //public class DbConThresholdType
    //{
    //    [Key]
    //    public int ID { get; set; }
    //    //FKs
    //    public virtual ICollection<DbConThreshold> DbThresholds { get; set; }
    //    //Properties
    //    public int Threshold { get; set; } // this is the int cast of the enum
    //    public string ThresholdName { get; set; }
    //    public int ThresholdType { get; set; }
    //}

    public class DbBeamAlias
    {
        [Key]
        public int ID { get; set; }
        [ForeignKey("DbBeam")]
        public int BeamID { get; set; }
        public virtual DbBeam DbBeam { get; set; }
        public string EclipseFieldId { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class DbStructureAlias
    {
        [Key]
        public int ID { get; set; }
        public string EclipseStructureId { get; set; }
        public int DisplayOrder { get; set; }
        [ForeignKey("DbProtocolStructure")]
        public int ProtocolStructureId { get; set; }
        public virtual DbProtocolStructure DbProtocolStructure { get; set; }
    }
    public class DbProtocolStructure
    {
        [Key]
        public int ID { get; set; }
        [ForeignKey("DbStructureLabel")]
        public int StructureLabelID { get; set; }
        public virtual DbStructureLabel DbStructureLabel { get; set; }
        [ForeignKey("DbLibraryProtocol")]
        public int ProtocolID { get; set; }
        public virtual DbProtocol DbLibraryProtocol { get; set; }
        public virtual DbStructureChecklist DbStructureChecklist { get; set; }
        //Data
        public string ProtocolStructureName { get; set; }
        public double? AlphaBetaRatioOverride { get; set; } = null;
        public bool isException { get; set; }
        public int DisplayOrder { get; set; }
        //
        public virtual ICollection<DbStructureAlias> DbStructureAliases { get; set; }
        public virtual ICollection<DbSessionProtocolStructure> DbSessionProtocolStructures { get; set; }
        public virtual ICollection<DbArtifact> DbArtifacts { get; set; }
    }

    public class DbSessionProtocolStructure : DbProtocolStructure
    {
        [ForeignKey("DbSession")]
        public int SessionId { get; set; }
        public virtual DbSession DbSession { get; set; }
        // Data
        public string AssignedEclipseId { get; set; }
        public string AssignedEclipseLabel { get; set; }
        public string AssignedEclipseStructureSetUID { get; set; }
        public int ParentProtocolStructure_Id { get; set; }
    }

    public class DbSession
    {
        [Key]
        public int ID { get; set; }
        [ForeignKey("DbSessionProtocol")]
        public int SessionProtocolId { get; set; }
        public virtual DbSessionProtocol DbSessionProtocol { get; set; }
        public virtual ICollection<DbSessionProtocolStructure> SessionProtocolStructures { get; set; }
        public virtual ICollection<DbSessionComponent> SessionComponents { get; set; }
        public virtual ICollection<DbSessionConstraint> SessionConstraints { get; set; }
        public virtual ICollection<DbAssessment> SessionAssessments { get; set; }
        public virtual ICollection<DbPlanAssociation> SessionPlans { get; set; }

        // Data
        public string PID { get; set; }
        public string SessionComment { get; set; }
        public string SessionCreator { get; set; }
        public string SessionDateTime { get; set; }

        public string SessionStructureSetUID { get; set; }
    }

}